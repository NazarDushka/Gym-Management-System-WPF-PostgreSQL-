using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBManager
{
    internal class Functions
    {
        private static string connectionString = "Host=localhost;Port=5436;Username=postgres;Password=12345;Database=projektSilownia";

        //function to login user
        public static bool Login(string name, string email)
        {
            bool Access =false;
            string query;
            using (var connection = new Npgsql.NpgsqlConnection(connectionString))
            {
                
                connection.Open();

                query = "SELECT COUNT(*) FROM pracownicy WHERE LOWER(imie) = LOWER(@imie) AND LOWER(email) = LOWER(@email)";

                using (var command = new Npgsql.NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("imie", name);
                    command.Parameters.AddWithValue("email", email);
                    long count = (long)command.ExecuteScalar();
                    if (count > 0)
                    {
                        Access = true;
                    }
                }

 
                return Access;
            }
        }

        //function to search in table
        public static DataTable Search(string tableName, string column, string value, bool isExact)
        {
            DataTable dataTable = new DataTable();

            using (var connection = new Npgsql.NpgsqlConnection(connectionString))
            {
                connection.Open();
                string query;

                if (isExact)
                {

                    query = $"SELECT * FROM \"{tableName}\" WHERE \"{column}\" = @value";
                }
                else
                {
                    query = $"SELECT * FROM \"{tableName}\" WHERE \"{column}\"::text ILIKE @value";
                }

                using (var command = new Npgsql.NpgsqlCommand(query, connection))
                {
                    if (isExact)
                    {
                        command.Parameters.AddWithValue("value", value);
                    }
                    else
                    {
                        command.Parameters.AddWithValue("value", $"%{value}%");
                    }

                    using (var adapter = new Npgsql.NpgsqlDataAdapter(command))
                    {
                        adapter.Fill(dataTable);
                    }
                }
            }
            return dataTable;
        }

        //function to get all table names
        public static string[] GetAllTablesNames()
        {
            List<string> tables = new List<string>();
            using (var connection = new Npgsql.NpgsqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT table_name FROM information_schema.tables WHERE table_schema = 'public'";
                using (var command = new Npgsql.NpgsqlCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            tables.Add(reader.GetString(0));
                        }
                    }
                }
            }
            tables.Sort();
            return tables.ToArray();
        }


        /* 
         CRUD OPERATIONS
         */

        //function to get entire table
        public static DataTable GetTable(string name)
        {
            DataTable table = new DataTable();

            using (var connection = new Npgsql.NpgsqlConnection(connectionString))
            {
                connection.Open();
                string query = $"SELECT * FROM {name} ORDER BY 1 ASC";
                using (var command = new Npgsql.NpgsqlCommand(query, connection))
                {
                    using (var adapter = new Npgsql.NpgsqlDataAdapter(command))
                    {
                        adapter.Fill(table);
                    }
                }
            }


            return table;
        }

        public static void DeleteRow(string tableName, string idColumnName, object idValue)
        {
            using (var connection = new Npgsql.NpgsqlConnection(connectionString))
            {
                connection.Open();
                string query = $"DELETE FROM {tableName} WHERE {idColumnName} = @id";

                using (var command = new Npgsql.NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", idValue);
                    command.ExecuteNonQuery();
                }
            }
        }

        public static void UpdateRow(string tableName, string idColumnName, object idValue, Dictionary<string,object> updatedValues)
        {
            using (var connection = new Npgsql.NpgsqlConnection(connectionString))
            {
                connection.Open();
                var setClauses = updatedValues.Select(kv => $"{kv.Key} = @{kv.Key}");
                string setClause = string.Join(", ", setClauses);
                string query = $"UPDATE {tableName} SET {setClause} WHERE {idColumnName} = @IdValue";
                var command = new Npgsql.NpgsqlCommand(query, connection);
                foreach (var kv in updatedValues)
                {
                    command.Parameters.AddWithValue($"@{kv.Key}", kv.Value);
                }
                command.Parameters.AddWithValue("@IdValue", idValue);
                command.ExecuteNonQuery();
            }
        }

        public static void AddRow(string tableName, Dictionary<string, object> values)
        {
            using (var connection = new Npgsql.NpgsqlConnection(connectionString))
            {
                connection.Open();
                var columns = string.Join(", ", values.Keys);
                var parameterNames = string.Join(", ", values.Keys.Select(k => "@" + k));
                string query = $"INSERT INTO {tableName} ({columns}) VALUES ({parameterNames})";
                var command = new Npgsql.NpgsqlCommand(query, connection);
                foreach (var kv in values)
                {
                    command.Parameters.AddWithValue($"@{kv.Key}", kv.Value);
                }
                command.ExecuteNonQuery();
            }
        }

        /*
         SCHEMA MODIFICATION OPERATIONS
         */
        public static void AddColumn(string tableName, string columnName, string dataType)
        {
            using (var connection = new Npgsql.NpgsqlConnection(connectionString))
            {
                connection.Open();
                string query = $"ALTER TABLE \"{tableName}\" ADD COLUMN \"{columnName}\" {dataType}";

                using (var command = new Npgsql.NpgsqlCommand(query, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        public static void DropColumn(string tableName, string columnName)
        {
            using (var connection = new Npgsql.NpgsqlConnection(connectionString))
            {
                connection.Open();
                string query = $"ALTER TABLE \"{tableName}\" DROP COLUMN \"{columnName}\"";

                using (var command = new Npgsql.NpgsqlCommand(query, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        public static void RenameColumn(string tableName, string oldColumnName, string newColumnName)
        {
            using (var connection = new Npgsql.NpgsqlConnection(connectionString))
            {
                connection.Open();
                string query = $"ALTER TABLE \"{tableName}\" RENAME COLUMN \"{oldColumnName}\" TO \"{newColumnName}\"";

                using (var command = new Npgsql.NpgsqlCommand(query, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }


        //public static void RenameTable(string oldTableName, string newTableName)
        //{
        //    using (var connection = new Npgsql.NpgsqlConnection(connectionString))
        //    {
        //        connection.Open();
        //        string query = $"ALTER TABLE {oldTableName} RENAME TO {newTableName}";
        //        var command = new Npgsql.NpgsqlCommand(query, connection);
        //        command.ExecuteNonQuery();
        //    }
        //}

      
      
    }
}

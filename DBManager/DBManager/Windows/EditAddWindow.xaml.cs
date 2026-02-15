using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DBManager
{
    public partial class EditAddWindow : Window
    {
        private string _tableName;
        private DataRowView _rowToEdit;

        private Dictionary<string, Control> _inputs = new Dictionary<string, Control>();

        public EditAddWindow(string tableName, DataRowView row = null)
        {
            InitializeComponent();
            _tableName = tableName;
            _rowToEdit = row;

            if (_rowToEdit == null)
                Title = $"Dodawanie do tabeli: {tableName}";
            else
                Title = $"Edycja rekordu w tabeli: {tableName}";

            GenerateFields();
        }

        private void GenerateFields()
        {
            DataTable schema = Functions.GetTable(_tableName);

            foreach (DataColumn col in schema.Columns)
            {
                string colName = col.ColumnName;
                Type colType = col.DataType;

                if (_rowToEdit == null && (colName.ToLower() == "id" || col == schema.Columns[0]))
                    continue;

                Label lbl = new Label();
                lbl.Content = $"{colName} ({GetFriendlyTypeName(colType)})"; 
                lbl.FontWeight = FontWeights.Bold;
                fieldsPanel.Children.Add(lbl);

                Control inputControl = null;

                if (colType == typeof(bool))
                {
                    CheckBox chk = new CheckBox();
                    chk.Content = "Tak / Nie";
                    chk.Margin = new Thickness(0, 0, 0, 10);

                    if (_rowToEdit != null && _rowToEdit[colName] != DBNull.Value)
                    {
                        chk.IsChecked = (bool)_rowToEdit[colName];
                    }
                    inputControl = chk;
                }
                else if (colType == typeof(DateTime) || colType == typeof(DateOnly))
                {
                    DatePicker dp = new DatePicker();
                    dp.Margin = new Thickness(0, 0, 0, 10);
                    dp.Height = 30;

                    if (_rowToEdit != null && _rowToEdit[colName] != DBNull.Value)
                    {
                        var value = _rowToEdit[colName];

                        if (value is DateTime dt)
                        {
                            dp.SelectedDate = dt;
                        }
                        else if (value is DateOnly d)
                        {
                            dp.SelectedDate = d.ToDateTime(TimeOnly.MinValue);
                        }
                    }
                    inputControl = dp;
                }
                else
                {
                    TextBox txt = new TextBox();
                    txt.Height = 25;
                    txt.Padding = new Thickness(2);
                    txt.Margin = new Thickness(0, 0, 0, 10);

                    if (_rowToEdit != null)
                    {
                        txt.Text = _rowToEdit[colName].ToString();

                        if (col == schema.Columns[0])
                        {
                            txt.IsEnabled = false;
                            txt.Background = Brushes.LightGray;
                        }
                    }
                    inputControl = txt;
                }

                fieldsPanel.Children.Add(inputControl);
                _inputs.Add(colName, inputControl);
            }
        }

        private string GetFriendlyTypeName(Type type)
        {
            if (type == typeof(int) || type == typeof(long)) return "Liczba";
            if (type == typeof(string)) return "Tekst";
            if (type == typeof(bool)) return "Logiczny";
            if (type == typeof(DateTime)) return "Data";
            return type.Name;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Dictionary<string, object> values = new Dictionary<string, object>();

                foreach (var item in _inputs)
                {
                    string colName = item.Key;
                    Control ctrl = item.Value;
                    object finalValue = null;

                    if (ctrl is CheckBox chk)
                    {
                        finalValue = chk.IsChecked ?? false;
                    }
                    else if (ctrl is DatePicker dp)
                    {
                        if (dp.SelectedDate.HasValue)
                        { 
                            DataTable schema = Functions.GetTable(_tableName);
                            Type targetType = schema.Columns[colName].DataType;

                            if (targetType == typeof(DateOnly))
                            {
                                finalValue = DateOnly.FromDateTime(dp.SelectedDate.Value);
                            }
                            else
                            {
                                finalValue = dp.SelectedDate.Value;
                            }
                        }
                        else
                        {
                            finalValue = DBNull.Value;
                        }
                    }
                    else if (ctrl is TextBox txt)
                    {
                        string text = txt.Text.Trim();
                        if (string.IsNullOrEmpty(text))
                            finalValue = DBNull.Value;
                        else
                            finalValue = text;
                    }

                    values.Add(colName, finalValue);
                }

                if (_rowToEdit == null)
                {
                    Functions.AddRow(_tableName, values);
                    MessageBox.Show("Dodano pomyślnie!", "Sukces");
                }
                else
                {
                    string idColName = _rowToEdit.Row.Table.Columns[0].ColumnName;
                    object idVal = _rowToEdit[0];

                    if (values.ContainsKey(idColName)) values.Remove(idColName);

                    Functions.UpdateRow(_tableName, idColName, idVal, values);
                    MessageBox.Show("Zapisano zmiany!", "Sukces");
                }

                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Błąd zapisu: " + ex.Message + "\nUpewnij się, że dane liczbowe są poprawne.", "Błąd");
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
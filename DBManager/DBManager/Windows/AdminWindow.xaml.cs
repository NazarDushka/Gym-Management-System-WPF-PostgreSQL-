using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace DBManager
{
    public partial class AdminWindow : Window
    {
        public AdminWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshTableList();
        }

        private void RefreshTableList()
        {
            try
            {
                cbTables.Items.Clear();
                string[] tables = Functions.GetAllTablesNames();
                foreach (var t in tables)
                {
                    cbTables.Items.Add(t);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Błąd ładowania tabel: " + ex.Message);
            }
        }

        private void CbTables_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadCurrentTable();
        }

        private void LoadCurrentTable()
        {
            if (cbTables.SelectedItem == null) return;

            string tableName = cbTables.SelectedItem.ToString();
            try
            {
                DataTable dt = Functions.GetTable(tableName);
                dataGrid.ItemsSource = dt.DefaultView; 
            }
            catch (Exception ex)
            {
                MessageBox.Show("Błąd pobierania danych: " + ex.Message);
            }
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadCurrentTable();
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            MainWindow login = new MainWindow();
            login.Show();
            this.Close();
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (cbTables.SelectedItem == null)
            {
                MessageBox.Show("Wybierz tabelę!");
                return;
            }

            if (dataGrid.SelectedItem == null)
            {
                MessageBox.Show("Zaznacz wiersz do usunięcia!");
                return;
            }

            var result = MessageBox.Show("Czy na pewno chcesz usunąć ten rekord?", "Potwierdzenie", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.No) return;

            try
            {
                DataRowView row = (DataRowView)dataGrid.SelectedItem;
                string tableName = cbTables.SelectedItem.ToString();

                string idColumnName = row.Row.Table.Columns[0].ColumnName;
                object idValue = row.Row[0];

                Functions.DeleteRow(tableName, idColumnName, idValue);

                LoadCurrentTable();
                MessageBox.Show("Usunięto pomyślnie!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Błąd usuwania: " + ex.Message);
            }
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (cbTables.SelectedItem == null)
            {
                MessageBox.Show("Wybierz tabelę!");
                return;
            }

            string tableName = cbTables.SelectedItem.ToString();

            EditAddWindow editWin = new EditAddWindow(tableName);

            editWin.ShowDialog();

            LoadCurrentTable();
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (cbTables.SelectedItem == null || dataGrid.SelectedItem == null)
            {
                MessageBox.Show("Wybierz tabelę i zaznacz wiersz!");
                return;
            }

            string tableName = cbTables.SelectedItem.ToString();
            DataRowView row = (DataRowView)dataGrid.SelectedItem;

            EditAddWindow editWin = new EditAddWindow(tableName, row);
            editWin.ShowDialog();

            LoadCurrentTable();
        }

        private void BtnAddCol_Click(object sender, RoutedEventArgs e)
        {
            if (cbTables.SelectedItem == null)
            {
                MessageBox.Show("Najpierw wybierz tabelę!");
                return;
            }

            string tableName = cbTables.SelectedItem.ToString();

            AddColumnWindow win = new AddColumnWindow(tableName);

            win.ShowDialog();

            LoadCurrentTable();
        }

        private void BtnDelCol_Click(object sender, RoutedEventArgs e)
        {
            if (cbTables.SelectedItem == null)
            {
                MessageBox.Show("Wybierz tabelę!");
                return;
            }

            DataView view = (DataView)dataGrid.ItemsSource;
            DataTable table = view.Table;

            string firstColumnName = table.Columns[0].ColumnName;

            List<string> columns = new List<string>();
            foreach (var col in dataGrid.Columns)
            {
                columns.Add(col.Header.ToString());
            }

            SelectColumnWindow selectWin = new SelectColumnWindow(columns);

            if (selectWin.ShowDialog() == true)
            {
                string colName = selectWin.SelectedColumn;

                if (colName == firstColumnName)
                {
                    MessageBox.Show($"Nie można usunąć kolumny '{colName}', ponieważ jest to klucz główny (pierwsza kolumna)!",
                                    "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var result = MessageBox.Show($"Czy na pewno usunąć kolumnę '{colName}'?", "Potwierdzenie", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        string tableName = cbTables.SelectedItem.ToString();
                        Functions.DropColumn(tableName, colName);
                        LoadCurrentTable();
                        MessageBox.Show("Kolumna usunięta.");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Błąd: " + ex.Message);
                    }
                }
            }
        }

        private void BtnRenameCol_Click(object sender, RoutedEventArgs e)
        {
            if (cbTables.SelectedItem == null)
            {
                MessageBox.Show("Wybierz tabelę!");
                return;
            }

            DataView view = (DataView)dataGrid.ItemsSource;
            DataTable table = view.Table;
            string firstColumnName = table.Columns[0].ColumnName;

            List<string> columns = new List<string>();
            foreach (var col in dataGrid.Columns)
            {
                columns.Add(col.Header.ToString());
            }

            SelectColumnWindow selectWin = new SelectColumnWindow(columns);

            if (selectWin.ShowDialog() == true)
            {
                string oldColName = selectWin.SelectedColumn;

                if (oldColName == firstColumnName)
                {
                    MessageBox.Show($"Nie można zmieniać nazwy kolumny '{oldColName}' (klucz główny).",
                                    "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string tableName = cbTables.SelectedItem.ToString();
                RenameColumnWindow renameWin = new RenameColumnWindow(tableName, oldColName);
                renameWin.ShowDialog();

                LoadCurrentTable();
            }
        }
    }
}
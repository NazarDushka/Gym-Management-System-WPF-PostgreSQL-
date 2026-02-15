using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace DBManager
{
    public partial class UserWindow : Window
    {
        private DataTable _originalTable;

        public UserWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
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
                MessageBox.Show("Błąd ładowania: " + ex.Message);
            }
        }

        private void CbTables_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbTables.SelectedItem == null) return;
            string tableName = cbTables.SelectedItem.ToString();

            try
            {
                _originalTable = Functions.GetTable(tableName);
                dataGrid.ItemsSource = _originalTable.DefaultView;

                cbColumns.Items.Clear();
                foreach (DataColumn col in _originalTable.Columns)
                {
                    cbColumns.Items.Add(col.ColumnName);
                }

                if (cbColumns.Items.Count > 0) cbColumns.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Błąd pobierania danych: " + ex.Message);
            }
        }

        private void RefreshTable(string tableName)
        {
            try
            {
                DataTable dt = Functions.GetTable(tableName);
                dataGrid.ItemsSource = dt.DefaultView;

                cbColumns.Items.Clear();
                foreach (DataColumn col in dt.Columns)
                {
                    cbColumns.Items.Add(col.ColumnName);
                }
                if (cbColumns.Items.Count > 0) cbColumns.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Błąd pobierania danych: " + ex.Message);
            }
        }

        private void BtnFilter_Click(object sender, RoutedEventArgs e)
        {
            if (cbTables.SelectedItem == null)
            {
                MessageBox.Show("Wybierz tabelę!");
                return;
            }
            if (cbColumns.SelectedItem == null)
            {
                MessageBox.Show("Wybierz kolumnę!");
                return;
            }

            string tableName = cbTables.SelectedItem.ToString();
            string colName = cbColumns.SelectedItem.ToString();
            string value = txtFilter.Text.Trim();
            bool isExact = chkExactMatch.IsChecked == true;

            if (string.IsNullOrEmpty(value))
            {
                RefreshTable(tableName);
                return;
            }

            try
            {
                DataTable resultTable = Functions.Search(tableName, colName, value, isExact);

                dataGrid.ItemsSource = resultTable.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd wyszukiwania: {ex.Message}\n(Sprawdź czy wpisujesz liczbę w pole liczbowe)");
            }
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            txtFilter.Text = "";
            chkExactMatch.IsChecked = false;

            if (cbTables.SelectedItem != null)
            {
                RefreshTable(cbTables.SelectedItem.ToString());
            }
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            MainWindow login = new MainWindow();
            login.Show();
            this.Close();
        }
    }
}
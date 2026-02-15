using System;
using System.Windows;
using System.Windows.Controls;

namespace DBManager
{
    public partial class AddColumnWindow : Window
    {
        private string _tableName;
        public AddColumnWindow(string tableName)
        {
            InitializeComponent();
            _tableName = tableName;
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            string colName = txtName.Text.Trim();

            string dataType = ((ComboBoxItem)cbType.SelectedItem).Tag.ToString();

            if (string.IsNullOrEmpty(colName))
            {
                MessageBox.Show("Proszę wpisać nazwę kolumny!", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (colName.Contains(" "))
            {
                MessageBox.Show("Nazwa kolumny nie może zawierać spacji!", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                Functions.AddColumn(_tableName, colName, dataType);

                MessageBox.Show($"Kolumna '{colName}' została dodana pomyślnie!", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Błąd bazy danych: " + ex.Message, "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
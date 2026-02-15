using System;
using System.Windows;

namespace DBManager
{
    public partial class RenameColumnWindow : Window
    {
        private string _tableName;
        private string _oldColName;

        public RenameColumnWindow(string tableName, string oldColName)
        {
            InitializeComponent();
            _tableName = tableName;
            _oldColName = oldColName;

            lblOldName.Text = $"Stara nazwa: {_oldColName}";
            txtNewName.Text = _oldColName;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            string newName = txtNewName.Text.Trim();

            if (string.IsNullOrEmpty(newName))
            {
                MessageBox.Show("Wpisz nową nazwę!");
                return;
            }

            if (newName == _oldColName)
            {
                this.Close();
                return;
            }

            try
            {
                Functions.RenameColumn(_tableName, _oldColName, newName);
                MessageBox.Show("Nazwa zmieniona pomyślnie!", "Sukces");
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Błąd zmiany nazwy: " + ex.Message);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
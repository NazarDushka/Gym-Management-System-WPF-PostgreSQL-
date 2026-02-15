using System.Collections.Generic;
using System.Windows;

namespace DBManager
{
    public partial class SelectColumnWindow : Window
    {
        public string SelectedColumn { get; private set; }

        public SelectColumnWindow(List<string> columns)
        {
            InitializeComponent();

            foreach (string col in columns)
            {
                cbColumns.Items.Add(col);
            }

            if (cbColumns.Items.Count > 0) cbColumns.SelectedIndex = 0;
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            if (cbColumns.SelectedItem == null)
            {
                MessageBox.Show("Wybierz kolumnę!");
                return;
            }

            SelectedColumn = cbColumns.SelectedItem.ToString();
            this.DialogResult = true;
            this.Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
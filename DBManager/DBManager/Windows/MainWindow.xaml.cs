using System.Data;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Markup;

namespace DBManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string name = txtImie.Text.Trim();
                string email = txtEmail.Text.Trim();

                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email))
                {
                    MessageBox.Show("Proszę wypełnić wszystkie pola!", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                bool access = Functions.Login(name, email);

                if (access == true)
                {
                    MessageBox.Show("Witaj, Administratorze!", "Sukces");
                    AdminWindow adminWin = new AdminWindow();
                    adminWin.Show();

                    this.Close();
                }
                else
                {
                    MessageBox.Show("Błędne dane logowania!", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Błąd podczas logowania: " + ex.Message, "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnGuest_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Wchodzisz w trybie podglądu (Gość).", "Informacja", MessageBoxButton.OK, MessageBoxImage.Information);


            UserWindow userWin = new UserWindow();
            userWin.Show();

            this.Close();
        }
    }
}
    

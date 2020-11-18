using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using DatabaseConnectionLib;

namespace Restaurant_Manager
{
    /// <summary>
    /// Логика взаимодействия для WindowAddPersonal.xaml
    /// </summary>

    public partial class WindowAddPersonal : Window
    {
        public bool PasswordCorrect = false;

        public WindowAddPersonal()
        {
            InitializeComponent();
        }

        private void CreateNewPersonal(object sender, RoutedEventArgs e)
        {
            if (textBoxLogin.Text.Length > 0 && DBConnector.AuthLogin(textBoxPass.Text) == null)
            {
                DBConnector.AddOficiant(textBoxLogin.Text, textBoxPass.Text);
                Close();
            }
        }


        Regex passwordRegex = new Regex(@"^\d{1,4}$");
        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Match match = passwordRegex.Match(e.Text);

            if ((sender as TextBox).Text.Length >= 4 || !match.Success)
            {
                e.Handled = true;
                return;
            }

            if ((sender as TextBox).Text.Length == 3)
            {
                if (DBConnector.AuthLogin(((sender as TextBox).Text) + e.Text) != null)
                {
                    texbBoxPassBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(255, 0, 0));
                    PasswordCorrect = false;
                }
                else
                {
                    PasswordCorrect = true;
                    texbBoxPassBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(0, 128, 0));
                    if (textBoxLogin.Text.Length > 0)  buttonCreateBorder.IsEnabled = true;
                }
            }
        }

        private void PasswordChanged(object sender, TextChangedEventArgs e)
        {
            if ((sender as TextBox).Text.Length != 4)
            {
                texbBoxPassBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(171, 173, 179));
                buttonCreateBorder.IsEnabled = false;
                PasswordCorrect = false;
            }
        }


        Regex loginRegex = new Regex(@"^[a-zA-Zа-яА-Я]$");

        private void TextBox_PreviewTextInput_1(object sender, TextCompositionEventArgs e)
        {
            Match match = loginRegex.Match(e.Text);

            if (!match.Success)
            {
                e.Handled = true;
                return;
            }

            if ((sender as TextBox).Text.Length >= 0 && PasswordCorrect) 
                buttonCreateBorder.IsEnabled = true;
            else buttonCreateBorder.IsEnabled = false;
        }

        private void LoginChanged(object sender, TextChangedEventArgs e)
        {
            if ((sender as TextBox).Text.Length > 0 && PasswordCorrect)
                buttonCreateBorder.IsEnabled = true; 
            else buttonCreateBorder.IsEnabled = false;
        }
    }

    public class PersonalCreateEventArgs
    {
        public string Login { get; }
        public string Password { get; }

        public PersonalCreateEventArgs(string login, string pass)
        {
            Login = login;
            Password = pass;
        }
    }
}

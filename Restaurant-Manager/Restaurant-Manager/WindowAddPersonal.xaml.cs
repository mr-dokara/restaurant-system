using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using DatabaseConnectionLib;

namespace Restaurant_Manager
{
    public partial class WindowAddPersonal : Window
    {
        public WindowAddPersonal()
        {
            InitializeComponent();
        }


        private void CreateNew_ButtonOnClick(object sender, RoutedEventArgs e)
        {
            if (textBoxLogin.Text.Length > 0 && DBConnector.AuthLogin(textBoxPass.Text) == null)
            {
                DBConnector.AddOficiant(textBoxLogin.Text, textBoxPass.Text);
                Close();
            }
        }


        #region TextBox - Login

        private readonly Regex loginRegex = new Regex(@"^[a-zA-Zа-яА-Я]$");

        private void Login_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var match = loginRegex.Match(e.Text);
            if (!match.Success) e.Handled = true;
        }

        private void Login_Changed(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(textBoxLogin.Text) && PasswordCorrect)
                buttonCreateBorder.IsEnabled = true;
            else buttonCreateBorder.IsEnabled = false;
        }

        private void Login_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space) e.Handled = true;
        }

        #endregion


        #region TextBox - Password

        private bool PasswordCorrect;
        private readonly Regex passwordRegex = new Regex(@"^[0-9]$");

        private void Password_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var match = passwordRegex.Match(e.Text);
            if (!match.Success) e.Handled = true;
        }

        private void Password_Changed(object sender, TextChangedEventArgs e)
        {
            if (textBoxPass.Text.Length == 4)
            {
                if (DBConnector.AuthLogin(textBoxPass.Text) != null)
                {
                    texbBoxPassBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(255, 0, 0));
                    PasswordCorrect = false;
                }
                else
                {
                    texbBoxPassBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(0, 128, 0));
                    if (!string.IsNullOrWhiteSpace(textBoxLogin.Text)) buttonCreateBorder.IsEnabled = true;
                    PasswordCorrect = true;
                }
            }
            else
            {
                texbBoxPassBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(171, 173, 179));
                buttonCreateBorder.IsEnabled = false;
                PasswordCorrect = false;
            }
        }

        #endregion
    }
}
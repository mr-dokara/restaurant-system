using System.Linq;
using DatabaseConnectionLib;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Restaurant_Manager
{
    public partial class WindowEditPersonal : Window
    {
        public string Login;

        public WindowEditPersonal(string login)
        {
            InitializeComponent();
            Login = login;
            textBoxLogin.Text = login;
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (DataIsValid)
            {
                DBConnector.RemoveOficiant(Login);
                DBConnector.AddOficiant(textBoxLogin.Text, textBoxPass.Text);

                Close();
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
            => Close();

        // Проверка правильности заполнения полей
        #region Check Valid Data

        private bool DataIsValid
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(textBoxLogin.Text)
                    && textBoxPass.Text.Length == 4 && IsPasswordCorrect || 
                    DBConnector.AuthLogin(textBoxPass.Text) == Login) return true;
                return false;
            }
        }

        private bool IsPasswordCorrect
        {
            get
            {
                if (DBConnector.AuthLogin(textBoxPass.Text) == null) return true;
                return false;
            }
        }

        private readonly Regex passwordRegex = new Regex(@"^[0-9]$");

        private void textBoxPass_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var match = passwordRegex.Match(e.Text);
            if (!match.Success) e.Handled = true;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if ((sender as TextBox).Name == "textBoxPass")
            {
                if (textBoxPass.Text.Length == 4)
                {
                    if (!IsPasswordCorrect && DBConnector.AuthLogin(textBoxPass.Text) != Login) borderTexbBoxPass.BorderBrush = new SolidColorBrush(Color.FromRgb(255, 0, 0));
                    else borderTexbBoxPass.BorderBrush = new SolidColorBrush(Color.FromRgb(0, 128, 0));
                }
                else borderTexbBoxPass.BorderBrush = new SolidColorBrush(Color.FromRgb(171, 173, 179));
            }

            if (DataIsValid)
            {
                borderBtnEdit.IsEnabled = true;
                backgroundBtnEdit.Visibility = Visibility.Hidden;
            }
            else
            {
                borderBtnEdit.IsEnabled = false;
                backgroundBtnEdit.Visibility = Visibility.Visible;
            }
        }

        private void textBoxPass_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space) e.Handled = true;
        }


        private readonly Regex loginRegex = new Regex(@"^[a-zA-Zа-яА-Я.]$");

        private void textBoxLogin_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var match = loginRegex.Match(e.Text);
            if (!match.Success) e.Handled = true;
        }

        #endregion

        // Визуализация незаполненных полей при наведении на кнопку "Добавить"
        #region Visual Invalid Data

        private void btnEdit_MouseEnter(object sender, MouseEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBoxLogin.Text)) textBoxLoginBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(255, 0, 0));
            if (textBoxPass.Text.Length < 4 || !IsPasswordCorrect) borderTexbBoxPass.BorderBrush = new SolidColorBrush(Color.FromRgb(255, 0, 0));
        }

        private void btnEdit_MouseLeave(object sender, MouseEventArgs e)
        {
            textBoxLoginBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(171, 173, 179));
            borderTexbBoxPass.BorderBrush = new SolidColorBrush(Color.FromRgb(171, 173, 179));
        }

        #endregion
    }
}
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
        private string oldLogin;

        public WindowEditPersonal(string login)
        {
            InitializeComponent();
            oldLogin = login;
            textBoxLogin.Text = login;
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            textBoxLogin.Text = textBoxLogin.Text.Trim();

            if (DataIsValid)
            {
                DBConnector.RemoveOficiant(oldLogin);
                DBConnector.AddOficiant(textBoxLogin.Text, textBoxPass.Text);

                DialogResult = true;
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
                if (!string.IsNullOrWhiteSpace(textBoxLogin.Text) && textBoxLogin.Text.Count(x => char.IsLetter(x)) > 1
                    && textBoxPass.Text.Length == 4 && IsPasswordCorrect || 
                    DBConnector.AuthLogin(textBoxPass.Text) == oldLogin) return true;
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

        private readonly Regex passwordRegex = new Regex(@"[1-9]");

        private void textBoxPass_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var match = passwordRegex.Match(e.Text);
            if (!match.Success) e.Handled = true;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if ((sender as TextBox).Name == "textBoxPass")
            {
                MatchCollection matches;
                if (!string.IsNullOrWhiteSpace(textBoxPass.Text) &&
                    (matches = passwordRegex.Matches(textBoxPass.Text.Replace(" ", ""))).Count !=
                    textBoxPass.Text.Replace(" ", "").Length)
                {
                    textBoxPass.Text = string.Empty;
                    foreach (Match match in matches)
                    {
                        if (textBoxPass.Text.Length == 4) break;
                        textBoxPass.Text += match.Value;
                    }
                }

                if (textBoxPass.Text.Length == 4)
                {
                    if (!IsPasswordCorrect && DBConnector.AuthLogin(textBoxPass.Text) != oldLogin) borderTexbBoxPass.BorderBrush = new SolidColorBrush(Color.FromRgb(255, 0, 0));
                    else borderTexbBoxPass.BorderBrush = new SolidColorBrush(Color.FromRgb(0, 128, 0));
                }
                else borderTexbBoxPass.BorderBrush = new SolidColorBrush(Color.FromRgb(171, 173, 179));
            }
            if ((sender as TextBox).Name == "textBoxLogin")
            {
                MatchCollection matches;
                if (!string.IsNullOrWhiteSpace(textBoxLogin.Text) &&
                    (matches = loginRegex.Matches(textBoxLogin.Text.Replace(" ", ""))).Count !=
                    textBoxLogin.Text.Replace(" ", "").Length)
                {
                    textBoxLogin.Text = string.Empty;
                    foreach (Match match in matches)
                    { textBoxLogin.Text += match.Value; }

                    textBoxLogin.Text = textBoxLogin.Text.Trim();
                }
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


        private readonly Regex loginRegex = new Regex(@"[a-zA-Zа-яА-Я.\ ]");

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
            if (string.IsNullOrWhiteSpace(textBoxLogin.Text) || textBoxLogin.Text.Count(x => char.IsLetter(x)) <= 1) textBoxLoginBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(255, 0, 0));
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
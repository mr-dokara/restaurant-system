using System;
using System.Threading.Tasks;
using System.Windows;
using DatabaseConnectionLib;
using RestClient.Interfaces;

namespace RestClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool _loginFormWasHidden;

        public bool LoginFormWasHidden
        {
            get => _loginFormWasHidden;
            set
            {
                _loginFormWasHidden = value;

                if (value)
                {
                    Keyboard.Visibility = PasswordEntity.Visibility = Visibility.Hidden;
                    //TODO: Change window size and title.
                }

            }
        }

        public MainWindow()
        {
            InitializeComponent();
            Keyboard.Control = PasswordEntity;
            //TODO: попросить Никиту сделать чек дб на доступность.
        }

        public async void AuthenticationAsync(string password)
        {
            var login = await Task.Run(() => DBConnector.AuthLogin(password));

            if (login != null)
            {
                var officiant = new Officiant(login);
                LoginWith(officiant);
            }
            else
            {
                MessageBox.Show("Wrong password. Check your password and try again.", "Auth Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void LoginWith(Officiant officiant)
        {
            
        }
    }
}

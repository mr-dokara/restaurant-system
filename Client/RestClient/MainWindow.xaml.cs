using System;
using System.Threading.Tasks;
using System.Windows;
using DatabaseConnectionLib;

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
            throw new NotImplementedException();
            var login = await Task.Run(() => DBConnector.AuthLogin(password));

            //...

            
        }
    }
}

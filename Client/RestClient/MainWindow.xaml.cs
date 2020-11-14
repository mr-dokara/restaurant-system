using DatabaseConnectionLib;
using Logger;
using RestClient.Interfaces;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace RestClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Log.AddNote("Application opened.");
            Keyboard.Control = PasswordEntity;
            PasswordEntity.FourCharactersEntered += AuthenticationAsync;
            if (!DBConnector.IsConnected())
            {
                Log.AddNote("Can't connect to database.");
                MessageBox.Show(
                    "Подключение прервано. Возможно на сервере ведутся технические работы, приносим свои извинения.",
                    "Ошибка подключения",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }

            Log.AddNote("Connection successful.");
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
                PasswordEntity.Clear();
                Log.AddNote($"Trying to enter password \"{password}\".");
            }
        }

        private void LoginWith(Officiant officiant)
        {
            Log.AddNote($"Officiant {officiant.Name} logged in.");
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Log.AddNote("Login window closed.");
        }
    }
}

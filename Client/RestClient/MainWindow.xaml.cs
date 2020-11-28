using DatabaseConnectionLib;
using Logger;
using OfficiantLib;
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
            try
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
                    Close();
                }
            }
            catch (Exception e)
            {
                Log.AddNote(e.Message);
            }

            Log.AddNote("Connection successful.");
        }

        public async void AuthenticationAsync(string password)
        {
            try
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
            catch (Exception e)
            {
                Log.AddNote(e.Message);
            }
        }

        private void LoginWith(Officiant officiant)
        {
            Log.AddNote($"Officiant {officiant.Name} logged in.");
            new OrderWindow(officiant).Show();
            Close();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Log.AddNote("Login window closed.");
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            //Numpad 74-83
            //Other 34-43

            AddNumber(34, 43, e);
            AddNumber(74, 83, e);
        }

        private void AddNumber(int left, int right, System.Windows.Input.KeyEventArgs e)
        {
            if ((int)e.Key >= left && (int)e.Key <= right)
            {
                PasswordEntity.Text += (int)e.Key - left;
            }
        }
    }
}

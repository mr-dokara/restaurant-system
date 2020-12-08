using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace RestClient
{
    /// <summary>
    /// Interaction logic for ShowOrdersWindow.xaml
    /// </summary>
    public partial class ShowOrdersWindow : Window
    {
        private string _currentWaiter;

        public ShowOrdersWindow()
        {
            InitializeComponent();
        }

        public ShowOrdersWindow(OfficiantLib.Officiant officiant) : this()
        {
            SetButtons();
            _currentWaiter = officiant.Name;
        }

        private async void SetButtons()
        {
            await Task.Run(() =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    foreach (var order in DatabaseConnectionLib.DBConnector.GetOrders()
                        .Where(x => x.Waiter == _currentWaiter)
                        .Select(x => x))
                    {
                        var button = new Button
                        {
                            Content = order.TableNumber
                        };

                        Orders.Children.Add(button);
                    }
                });
            });
        }
    }
}
using RestClient.CustomControls;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

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
                        var ord = new OrderPreview(order)
                        {
                            Height = 138,
                            Width = 180
                        };
                        ord.ImageMouseDown += OrdOnImageMouseDown;

                        Orders.Children.Add(ord);
                    }
                });
            });

            LoadingCircle.Visibility = Visibility.Hidden;
        }

        private void OrdOnImageMouseDown(object sender, EventArgs e)
        {
            var orderPreview = sender as OrderPreview;
            Application.Current.Dispatcher.InvokeAsync(() => Orders.Children.Remove(orderPreview));
        }
    }
}
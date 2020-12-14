using System;
using System.Linq;
using OfficiantLib;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using DatabaseConnectionLib;

namespace CookerClient.CustomControls
{
    /// <summary>
    /// Interaction logic for OrderView.xaml
    /// </summary>
    public partial class OrderView : UserControl
    {
        public OrderData Data { get; }
        public event EventHandler OrderClosed;
        public OrderView(OrderData data)
        {
            InitializeComponent();
            Data = data;
            SetPositions(data);
        }

        private async void SetPositions(OrderData data)
        {
            await Task.Run(() =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    TableNumber.Content = data.Order.TableIndex;

                    foreach (var dish in data.Order.Dishes)
                    {
                        var button = new Button
                        {
                            Content = dish.Name
                        };

                        button.Click += (sender, args) =>
                        {
                            MessageBox.Show($"Комментарий: {dish.Comment}\nКоличество: {dish.Count}", "", MessageBoxButton.OK);
                        };

                        Positions.Children.Add(button);
                    }
                });
            });
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            var current = DBConnector
                .GetOrders().FirstOrDefault(x => x.TableNumber == Data.Order.TableIndex.ToString() && x.Waiter == Data.Officiant.Name);

            if (current == null) return;

            DBConnector.CloseOrder(current.Id);
            OrderClosed?.Invoke(this, EventArgs.Empty);
        }
    }
}
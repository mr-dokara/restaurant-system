using System.Linq;
using OfficiantLib;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace CookerClient.CustomControls
{
    /// <summary>
    /// Interaction logic for OrderView.xaml
    /// </summary>
    public partial class OrderView : UserControl
    {
        public OrderView(OrderData data)
        {
            InitializeComponent();
            SetPositions(data);
        }

        private async void SetPositions(OrderData data)
        {
            await Task.Run(() =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    TableNumber.Content = data.Order.TableIndex;

                    foreach (var button in data.Order.Dishes.Select(dish => new Button
                    {
                        Content = dish.Name
                    }))
                    {
                        Positions.Children.Add(button);
                    }
                });
            });
        }
    }
}
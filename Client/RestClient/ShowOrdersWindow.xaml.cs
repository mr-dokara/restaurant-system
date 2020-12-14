using RestClient.CustomControls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using DatabaseConnectionLib;
using Dish = OfficiantLib.Dish;

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

        private async void OrdOnImageMouseDown(object sender, EventArgs e)
        {
            var orderPreview = sender as OrderPreview;
            await Application.Current.Dispatcher.InvokeAsync(() => Orders.Children.Remove(orderPreview));

            if (orderPreview == null) return;
            var order = orderPreview.Order;
            var dishes = await Task.Run(() =>
            {
                return (from orderListDish in order.ListDishes
                    let dishName = orderListDish.Key
                    let count = orderListDish.Value
                    let price = DBConnector.GetDishes().FirstOrDefault(x => x.Name == dishName)?.Price
                    where price != null
                    select new Dish(dishName, (float) price, count)).ToList();
            });

            var cheque = new Cheque(_currentWaiter, int.Parse(orderPreview.Order.TableNumber), dishes);
            MessageBox.Show(cheque.ToString(), "Расчет", MessageBoxButton.OK);
        }
    }

    internal class Cheque : IEnumerable<Dish>
    {
        public string WaiterName { get; }
        public int TableIndex { get; }
        private readonly IEnumerable<Dish> _dishes;

        public Cheque() : this(null, 0, null)
        { }

        public Cheque(string waiter, int tableIndex, IEnumerable<Dish> dishes)
        {
            WaiterName = waiter;
            TableIndex = tableIndex;
            _dishes = dishes;
        }

        public override string ToString()
        {
            var sb = new StringBuilder($"=======ЧЕК=======\nОфициант: {WaiterName}\nСтол: {TableIndex}\n=====Позиции=====\n");

            var maxLenOfWord = _dishes.Select(dish => dish.Name.Length).Prepend(0).Max();
            var sum = 0f;
            foreach (var dish in _dishes)
            {
                var minusCounter = maxLenOfWord - dish.Name.Length + 2;
                sb.Append(dish.Name);
                sb.Append('-', minusCounter);
                sb.Append($"{dish.Count}x{dish.Price * dish.Count:F2} руб.\n");
                sum += dish.Price * dish.Count;
            }

            sb.Append("=================\n");
            sb.Append($"ИТОГО: {sum:F2}");
            return sb.ToString();
        }

        public IEnumerator<Dish> GetEnumerator()
        {
            return _dishes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
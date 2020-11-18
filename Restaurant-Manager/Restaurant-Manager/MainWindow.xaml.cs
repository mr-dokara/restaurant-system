using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DatabaseConnectionLib;
using Org.BouncyCastle.Asn1.Cms;

namespace Restaurant_Manager
{
    public partial class MainWindow : Window
    {
        private DataType currentData = DataType.None;

        public MainWindow()
        {
            InitializeComponent();
        }


        private void OpenPersonalMenu(object sender, RoutedEventArgs e)
        {
            var table = DBConnector.GetOficiants();
            var tablePersonal = new List<string>(table.Count());
            foreach (string s in table) { tablePersonal.Add(s); }
            listBoxPersonal.ItemsSource = tablePersonal;
            listBoxPersonal.Visibility = Visibility.Visible;
            DataGridOrders.Visibility = Visibility.Hidden;
            labelNameTable.Content = "Список сотрудников";
            currentData = DataType.Personal;
            if (table.Count() > 1) btnDeleteAll.IsEnabled = true;

        }

        private void OpenOrdersMenu(object sender, RoutedEventArgs e)
        {
            var table = DBConnector.GetOrders();
            SyncOrdersDB(true);
            DataGridOrders.Visibility = Visibility.Visible;
            listBoxPersonal.Visibility = Visibility.Hidden;
            labelNameTable.Content = "Список заказов";
            currentData = DataType.Orders;
            if (table.Count() > 1) btnDeleteAll.IsEnabled = true;
        }

        private void SyncOrdersDB(bool reverse = false)
        {
            var dbOrders = DBConnector.GetOrders();

            if (!reverse)
            {
                var orders = DataGridOrders.ItemsSource.Cast<Order>();
                bool changed = false;

                foreach (Order order in orders)
                {
                    Order currentOrder;
                    if (order.Id == null || (currentOrder = dbOrders.FirstOrDefault(x => x.Id == order.Id)) == null)
                    {
                        if (order.Id == null)
                        {
                            var currentTime = DateTime.Now;
                            order.Id = $"{currentTime.Day:00}{currentTime.Month:00}{(currentTime.Year % 100):00}" +
                                       $"{currentTime.Hour:00}{currentTime.Minute:00}{currentTime.Second:00}";
                        }
                        DBConnector.CreateOrder(order);
                        changed = true;
                    }
                    else
                    {
                        if (currentOrder.Phone != order.Phone)
                        {
                            DBConnector.ChangeOrderParameter("consumerphone", order.Id, order.Phone);
                            changed = true;
                        }

                        if (currentOrder.ListDishes != order.ListDishes)
                        {
                            DBConnector.ChangeOrderParameter("listdishes", order.Id, order.ListDishes);
                            changed = true;
                        }

                        if (currentOrder.DeliveryOption != order.DeliveryOption)
                        {
                            DBConnector.ChangeOrderParameter("deliveryoption", order.Id, order.DeliveryOption);
                            changed = true;
                        }

                        if (currentOrder.Status != order.Status)
                        {
                            DBConnector.ChangeOrderParameter("status", order.Id, order.Status);
                            changed = true;
                        }
                    }
                }

                if (changed) SyncOrdersDB(true);

                foreach (Order order in dbOrders)
                {
                    if (!orders.Any(x => x.Id == order.Id))
                        DBConnector.DeleteOrder(order.Id);
                }
            }
            else DataGridOrders.ItemsSource = dbOrders;
        }

        private bool IsSynchronized
        {
            get
            {
                var dbOrders = DBConnector.GetOrders();
                var orders = DataGridOrders.ItemsSource.Cast<Order>();

                if (Math.Abs(dbOrders.Count() - orders.Count()) > 1) SyncOrdersDB(true);

                foreach (Order order in orders)
                {
                    Order currentOrder;
                    if ((currentOrder = dbOrders.FirstOrDefault(x => x.Id == order.Id)) == null || 
                        currentOrder.Phone != order.Phone || currentOrder.ListDishes != order.ListDishes ||
                        currentOrder.DeliveryOption != order.DeliveryOption || currentOrder.Status != order.Status) return false;
                }

                foreach (Order order in dbOrders)
                {
                    Order currentOrder;
                    if ((currentOrder = orders.FirstOrDefault(x => x.Id == order.Id)) == null ||
                        currentOrder.Phone != order.Phone || currentOrder.ListDishes != order.ListDishes ||
                        currentOrder.DeliveryOption != order.DeliveryOption || currentOrder.Status != order.Status) return false;
                }

                return true;
            }
        }

        private void OrdersChanged(object sender, EventArgs e)
        {
            if (!IsSynchronized)
                SyncOrdersDB();
        }

        private void btnDeleteAll_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Вы действительно хотите удалить все записи?", "Внимание",
                MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);
            if (result == MessageBoxResult.Yes)
            {
                if (currentData == DataType.Orders)
                {
                    var table = DBConnector.GetOrders();
                    foreach (Order order in table) 
                    { DBConnector.DeleteOrder(order.Id); }
                    SyncOrdersDB(true);
                }
                else if (currentData == DataType.Personal)
                {
                    var table = DBConnector.GetOficiants();
                    foreach (string login in table)
                    { DBConnector.RemoveOficiant(login); }

                    table = DBConnector.GetOficiants();
                    var tablePersonal = new List<string>(table.Count());
                    foreach (string s in table) { tablePersonal.Add(s); }
                    listBoxPersonal.ItemsSource = tablePersonal;
                }

                btnDeleteAll.IsEnabled = false;
            }
        }

        class DataPersonal
        {
            public string Login;

            public DataPersonal(string login)
            {
                Login = login;
            }
        }
    }
}

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
        public MainWindow()
        {
            InitializeComponent();
        }


        private void OpenPersonalMenu(object sender, RoutedEventArgs e)
        {
            DataGridPersonal.ItemsSource = DBConnector.GetOficiants();
            DataGridPersonal.Visibility = Visibility.Visible;
            DataGridOrders.Visibility = Visibility.Hidden;
            labelNameTable.Content = "Список сотрудников";

        }

        private void OpenOrdersMenu(object sender, RoutedEventArgs e)
        {
            SyncOrdersDB(true);
            DataGridOrders.Visibility = Visibility.Visible;
            DataGridPersonal.Visibility = Visibility.Hidden;
            labelNameTable.Content = "Список заказов";
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
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
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

        private void ShowTable(DataType type)
        {
            DataGridOrders.Visibility = Visibility.Hidden;
            listBoxPersonal.Visibility = Visibility.Hidden;

            btnAddNew.IsEnabled = false;
            btnRefresh.IsEnabled = false;
            btnDeleteAll.IsEnabled = false;


            if (type == DataType.Personal)
            {
                currentData = DataType.Personal;
                labelNameTable.Content = "Список официантов";

                var table = DBConnector.GetOficiants();
                SyncDB(true);

                listBoxPersonal.Visibility = Visibility.Visible;
                btnAddNew.IsEnabled = true;
                btnRefresh.IsEnabled = true;

                if (table.Count() > 0) btnDeleteAll.IsEnabled = true;
            }

            if (type == DataType.Orders)
            {
                currentData = DataType.Orders;
                labelNameTable.Content = "Список заказов";

                var table = DBConnector.GetOrders();
                SyncDB(true);

                DataGridOrders.Visibility = Visibility.Visible;
                btnAddNew.IsEnabled = true;
                btnRefresh.IsEnabled = true;

                if (table.Count() > 0) btnDeleteAll.IsEnabled = true;
            }
        }

        private void OpenPersonalMenu(object sender, RoutedEventArgs e)
            => ShowTable(DataType.Personal);

        private void OpenOrdersMenu(object sender, RoutedEventArgs e)
            => ShowTable(DataType.Orders);



        private void Orders_Changed(object sender, EventArgs e)
        {
            if (!IsSynchronized) SyncDB();   
        }

        #region Synchronization

        private void SyncDB(bool reverse = false)
        {
            if (currentData == DataType.Orders)
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
                                var st = order.Phone;
                                st = st.Replace("+", String.Empty);
                                st = st.Replace("(", String.Empty);
                                st = st.Replace(")", String.Empty);
                                st = st.Replace("-", String.Empty);
                                int k = 11 - st.Length;
                                for (int i = 0; i < k; i++) st = st.Insert(st.Length, "0");
                                st = st.Insert(0, "+");
                                st = st.Insert(2, "(");
                                st = st.Insert(6, ")");
                                st = st.Insert(10, "-");
                                st = st.Insert(13, "-");

                                DBConnector.ChangeOrderParameter("consumerphone", order.Id, st);
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

                    if (changed) SyncDB(true);

                    foreach (Order order in dbOrders)
                    {
                        if (!orders.Any(x => x.Id == order.Id))
                            DBConnector.DeleteOrder(order.Id);
                    }
                }
                else DataGridOrders.ItemsSource = dbOrders;
            }
            else if (currentData == DataType.Personal)
            {
                var dbPersonal = DBConnector.GetOficiants();
                listBoxPersonal.ItemsSource = dbPersonal;
            }
        }
        private bool IsSynchronized
        {
            get
            {
                if (currentData == DataType.Orders)
                {
                    var dbOrders = DBConnector.GetOrders();
                    var orders = DataGridOrders.ItemsSource.Cast<Order>();

                    if (Math.Abs(dbOrders.Count() - orders.Count()) > 1) SyncDB(true);

                    foreach (Order order in orders)
                    {
                        Order currentOrder;
                        if ((currentOrder = dbOrders.FirstOrDefault(x => x.Id == order.Id)) == null ||
                            currentOrder.Phone != order.Phone || currentOrder.ListDishes != order.ListDishes ||
                            currentOrder.DeliveryOption != order.DeliveryOption ||
                            currentOrder.Status != order.Status) return false;
                    }

                    foreach (Order order in dbOrders)
                    {
                        Order currentOrder;
                        if ((currentOrder = orders.FirstOrDefault(x => x.Id == order.Id)) == null ||
                            currentOrder.Phone != order.Phone || currentOrder.ListDishes != order.ListDishes ||
                            currentOrder.DeliveryOption != order.DeliveryOption ||
                            currentOrder.Status != order.Status) return false;
                    }

                    return true;
                }

                if (currentData == DataType.Personal)
                {
                    var dbPersonal = DBConnector.GetOficiants();
                    var Personal = DataGridOrders.ItemsSource.Cast<string>();

                    if (Math.Abs(dbPersonal.Count() - Personal.Count()) > 1) SyncDB(true);

                    foreach (string oficiant in Personal)
                    {
                        if (dbPersonal.FirstOrDefault(x => x == oficiant) == null) return false;
                    }

                    foreach (string oficiant in dbPersonal)
                    {
                        if (Personal.FirstOrDefault(x => x == oficiant) == null) return false;
                    }

                    return true;
                }
                return false;
            }
        }

        #endregion





        private void Clear_buttonOnClick(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Вы действительно хотите удалить все записи?", "Внимание",
                MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);
            if (result == MessageBoxResult.Yes)
            {
                if (currentData == DataType.Orders)
                {
                    var table = DBConnector.GetOrders();
                    foreach (Order order in table)
                    {
                        DBConnector.DeleteOrder(order.Id);
                    }

                    SyncDB(true);
                }
                else if (currentData == DataType.Personal)
                {
                    var table = DBConnector.GetOficiants();
                    foreach (string login in table)
                    {
                        DBConnector.RemoveOficiant(login);
                    }

                    table = DBConnector.GetOficiants();
                    var tablePersonal = new List<string>(table.Count());
                    foreach (string s in table)
                    {
                        tablePersonal.Add(s);
                    }

                    listBoxPersonal.ItemsSource = tablePersonal;
                }

                btnDeleteAll.Visibility = Visibility.Hidden;
            }
        }

        private void PersonalEdit_OnClick(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void PersonalDelete_OnClick(object sender, RoutedEventArgs e)
        {
            foreach (string item in listBoxPersonal.SelectedItems)
            {
                DBConnector.RemoveOficiant(item);
            }

            var table = DBConnector.GetOficiants();
            var tablePersonal = new List<string>(table.Count());
            foreach (string s in table)
            {
                tablePersonal.Add(s);
            }

            listBoxPersonal.ItemsSource = tablePersonal;
        }

        private void AddNew_ButtonOnClick(object sender, RoutedEventArgs e)
        {
            if (currentData == DataType.Orders)
            {
                var temp = DataGridOrders.ItemsSource.Cast<Order>().ToList();
                temp.Add(new Order());
                DataGridOrders.ItemsSource = temp;
                SyncDB();
            }

            if (currentData == DataType.Personal)
            {
                var window = new WindowAddPersonal();
                window.Owner = this;
                window.ShowDialog();
                SyncDB(true);
            }
        }

        private void Refresh_ButtonOnClick(object sender, RoutedEventArgs e)
        {
            if (refresh.Angle == 0 || refresh.Angle == 720)
            {
                refresh.BeginAnimation(RotateTransform.AngleProperty, new DoubleAnimation
                {
                    From = 0,
                    To = 720,
                    Duration = TimeSpan.FromSeconds(1)
                });

                SyncDB(true);
            }
        }


        private readonly Regex phoneRegex = new Regex(@"^[0-9]$");
        private void DataGridOrders_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (DataGridOrders.CurrentColumn.Header.ToString() == "Телефон")
            {
                var match = phoneRegex.Match(e.Text);
                if (!match.Success) e.Handled = true;
            }
        }
    }
}

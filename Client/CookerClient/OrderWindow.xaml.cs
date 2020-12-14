using Logger;
using OfficiantLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using CookerClient.CustomControls;

namespace CookerClient
{
    /// <summary>
    /// Interaction logic for OrderWindow.xaml
    /// </summary>
    public partial class OrderWindow : Window
    {
        public OrderWindow()
        {
            InitializeComponent();
            GetOrderDataAsync();
        }

        private async void GetOrderDataAsync()
        {
            try
            {
                var server = GetServer("127.0.0.1", 7777);
                server.Start();

                do
                {
                    var client = await server.AcceptTcpClientAsync();
                    var stream = client.GetStream();
                    var data = new byte[256];
                    var response = new StringBuilder();

                    await Task.Run(() =>
                    {
                        do
                        {
                            var bytes = stream.Read(data, 0, data.Length);
                            response.Append(Encoding.UTF8.GetString(data, 0, bytes));
                        } while (stream.DataAvailable);
                    });

                    stream.Close();
                    client.Close();

                    var dataTransformed = await DeserializeOrderDataAsync(response.ToString());
                    AddButton(dataTransformed);
                } while (true);
            }
            catch (Exception e)
            {
                Log.AddNote(e.Message);
                throw;
            }
        }

        private async void AddButton(OrderData data)
        {
            await Task.Run(() =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var orderView = new OrderView(data);
                    orderView.OrderClosed += OrderViewOnOrderClosed;
                    OrderPanel.Children.Add(orderView);
                });
            });
        }

        private async void OrderViewOnOrderClosed(object sender, EventArgs e)
        {
            if (!(sender is OrderView orderView)) return;

            var data = orderView.Data;

            await Task.Run(() =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    OrderPanel.Children.Remove(orderView);
                });
            });

            //MessageBox.Show(,
            //    "", MessageBoxButton.OK);
        }
    }

    internal class Cheque : IEnumerable<Dish>
    {
        public string WaiterName { get; private set; }
        public int TableIndex { get; private set; }
        private IEnumerable<Dish> _dishes;

        public Cheque()
        {
            WaiterName = null;
            _dishes = null;
            TableIndex = 0;
        }

        public IEnumerator<Dish> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
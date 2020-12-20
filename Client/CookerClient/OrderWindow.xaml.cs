﻿using CookerClient.CustomControls;
using DatabaseConnectionLib;
using Logger;
using MySql.Data.MySqlClient;
using OfficiantLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Dish = OfficiantLib.Dish;
using Order = DatabaseConnectionLib.Order;

namespace CookerClient
{
    /// <summary>
    /// Interaction logic for OrderWindow.xaml
    /// </summary>
    public partial class OrderWindow : Window
    {
        private readonly HashSet<Order> _currentOrders = new HashSet<Order>();

        public OrderWindow()
        {
            InitializeComponent();
            GetOrdersEvery5SecAsync();
            GetOrderDataAsync();
        }

        private async void GetOrdersEvery5SecAsync()
        {
            while (true)
            {
                await Task.Run(() => Thread.Sleep(5000));
                await GetOrdersFromDbAsync();
            }
        }
        
        private async Task GetOrdersFromDbAsync()
        {
            await Task.Run(() =>
            {
                try
                {
                    foreach (var order in DBConnector.GetOrders()
                        .Where(x => x.Status == "Confirmed" || x.Status == "Confirming").Select(x => x))
                    {
                        if (_currentOrders.Contains(_currentOrders.FirstOrDefault(x => x.Id == order.Id))) continue;

                        var waiterName = order.Waiter == string.Empty ? null : order.Waiter;
                        var dishes = (from orderListDish in order.ListDishes
                                      let dishName = orderListDish.Key
                                      let count = orderListDish.Value
                                      let price = 0
                                      select new Dish(dishName, price, count)).ToList();

                        var waiter = new Officiant(waiterName);
                        var ord = new OfficiantLib.Order(dishes,
                            int.TryParse(order.TableNumber, out var tableNum) ? tableNum : -1);
                        _currentOrders.Add(order);
                        var orderData = new OrderData(waiter, ord, order);

                        AddButtonAsync(orderData);
                    }
                }
                catch (MySqlException e)
                {
                    MessageBox.Show("Нет соединения с базой данных", "Ошибка соединения", MessageBoxButton.OK, MessageBoxImage.Error);
                    Log.AddNote(e.ToString());
                    Application.Current.Dispatcher.Invoke(Close);
                }
            });
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
                    _currentOrders.Add(dataTransformed.DbOrder);
                    AddButtonAsync(dataTransformed);
                } while (true);
            }
            catch (Exception e)
            {
                Log.AddNote(e.ToString());
                throw;
            }
        }

        private async void AddButtonAsync(OrderData data)
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

            await Task.Run(() =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    OrderPanel.Children.Remove(orderView);
                });
            });
        }
    }
}
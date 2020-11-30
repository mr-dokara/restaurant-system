using DatabaseConnectionLib;
using Logger;
using Newtonsoft.Json;
using OfficiantLib;
using RestClient.CustomControls;
using RestClient.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Dish = DatabaseConnectionLib.Dish;
using Order = OfficiantLib.Order;

namespace RestClient
{
    /// <summary>
    /// Interaction logic for OrderWindow.xaml
    /// </summary>
    public partial class OrderWindow : Window
    {
        private const double DefaultWidth = 962 / 3.0;
        private const int DefaultHeight = 197;
        private const int DefaultFontSize = 24;
        private const int DefaultBlockViewHeight = 55;
        private readonly Dictionary<string, HashSet<Dish>> _dishes;
        private Dictionary<Button, Dish> _buttons;
        private HashSet<DishInBlock> _order;
        private Order _currentOrder;
        private readonly Officiant _officiant;
        private TcpClient _client;
        private event EventHandler OrderCountChanged;

        public OrderWindow(Officiant officiant)
        {
            Log.AddNote("Order window opened.");
            InitializeComponent();
            _dishes = new Dictionary<string, HashSet<Dish>>();
            _order = new HashSet<DishInBlock>();
            OrderCountChanged += OnOrderCountChanged;
            _currentOrder = new Order();
            OfficiantName.Text = officiant.Name;
            _officiant = officiant;
            Timer();
            SetButtonsCategories();
        }

        private void OnOrderCountChanged(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.InvokeAsync(() =>
            {
                SendButton.IsEnabled = _order.Count != 0;
            });
        }

        private async void Timer()
        {
            Log.AddNote("Timer started.");
            const int oneSecond = 1000;
            await Task.Run(() =>
            {
                while (true)
                {
                    var time = DateTime.Now;
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Title = $"{time.Day:D2}.{time.Month:D2}.{time.Year:D2} {time.Hour:D2}:{time.Minute:D2}:{time.Second:D2}";
                    });
                    Thread.Sleep(oneSecond);
                }
            });
        }

        private async void SetButtonsCategories()
        {
            await Task.Run(GetCategories);
            await Task.Run(() =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Products.Children.Clear();

                    foreach (var button in _dishes.Keys.Select(category => new Button
                    {
                        Width = DefaultWidth,
                        Content = category,
                        Height = DefaultHeight,
                        FontSize = DefaultFontSize
                    }))
                    {
                        button.Click += Category_OnClick;
                        Products.Children.Add(button);
                        Log.AddNote($"Added button named {button.Content}.");
                    }
                });
            });

            LoadingCircle.Visibility = Visibility.Hidden;
        }

        private void GetCategories()
        {
            lock (_dishes)
            {
                if (_dishes.Count != 0) return;

                foreach (var dish in DBConnector.GetDishes())
                {
                    if (!_dishes.ContainsKey(dish.Category))
                        _dishes.Add(dish.Category, new HashSet<Dish>());

                    _dishes[dish.Category].Add(dish);
                    Log.AddNote($"{dish.Name} loaded. Category: {dish.Category}.");
                }
            }
        }

        private void Officiant_OnClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left) return;

            new MainWindow().Show();
            Log.AddNote("Order window closed.");
            Close();
        }

        private async void Category_OnClick(object sender, EventArgs e)
        {
            Products.Children.Clear();
            LoadingCircle.Visibility = Visibility.Visible;

            await Task.Run(() =>
            {
                lock (_dishes)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        _buttons = new Dictionary<Button, Dish>();

                        try
                        {
                            foreach (var dish in _dishes[((Button)sender).Content.ToString()])
                            {
                                var button = new Button
                                {
                                    Width = DefaultWidth,
                                    Content = dish.Name,
                                    Height = DefaultHeight,
                                    FontSize = DefaultFontSize,
                                };

                                _buttons[button] = dish;
                                button.Click += (o, args) => { DishButton_OnClick(o); };
                                Products.Children.Add(button);
                                Log.AddNote($"Added button named {button.Content}.");
                            }
                        }
                        catch (Exception exception)
                        {
                            Log.AddNote(exception.Message);
                            throw;
                        }
                    });

                }
            });

            LoadingCircle.Visibility = Visibility.Hidden;
        }

        //TODO: Re fuck toring.
        private void DishButton_OnClick(object o)
        {
            OrderProps.Reset();
            OrderProps.ChangeVisibilityStatus();

            OrderProps.SetAddEvent((obj, routedEvent) =>
            {
                OrderProps.ChangeVisibilityStatus();
                var currentDish = _buttons[(Button)o];
                var blockView = new BlockView(CancelButton_OnClick)
                {
                    Caption = currentDish.Name,
                    Price = $"{OrderProps.CountOfItems}x{currentDish.Price}",
                    Width = OrderPanel.ActualWidth,
                    Height = DefaultBlockViewHeight
                };
                OrderPanel.Children.Add(blockView);

                var block = new DishInBlock(blockView.CloseButton, currentDish,
                    OrderProps.CommentAbout, OrderPanel.Children.Count - 1);
                _order.Add(block);
                OrderCountChanged?.Invoke(this, EventArgs.Empty);
                Log.AddNote(
                    $"Block: {block.Position}:{block.Dish.Name}:{OrderProps.CountOfItems}");

                var dishToSend = new OfficiantLib.Dish(currentDish.Name,
                    currentDish.Price, OrderProps.CountOfItems)
                { Comment = OrderProps.CommentAbout };
                _currentOrder.Dishes.Add(dishToSend);
            });
        }

        private async void CancelButton_OnClick(object sender, RoutedEventArgs e)
        {
            DishInBlock removableDish = null;
            try
            {
                await Task.Run(() =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        if (!_order.TryGetValue(_order.First(x => x.CloseButton == (Button)sender), out removableDish))
                            throw new NullReferenceException();

                        OrderPanel.Children.RemoveAt(removableDish.Position);
                    });
                });

                await Task.Run(() =>
                {
                    lock (_order)
                    {
                        foreach (var dishInBlock in _order.Where(dishInBlock =>
                            dishInBlock.Position > removableDish.Position))
                        {
                            dishInBlock.Position--;
                        }

                        _order.Remove(removableDish);
                        OrderCountChanged?.Invoke(this, EventArgs.Empty);
                    }
                });
            }
            catch (Exception ex)
            {
                Log.AddNote(ex.Message);
                throw;
            }
        }

        private static string GetJsonStringOfSerializedObject(object obj)
        {
            StringBuilder serializedObject;
            try
            {
                var serialization = new JsonSerializer();
                serializedObject = new StringBuilder();
                serialization.Serialize(new StringWriter(serializedObject), obj);
            }
            catch (Exception e)
            {
                Log.AddNote(e.Message);
                throw;
            }

            return serializedObject.ToString();
        }

        private void SendButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                _client = new TcpClient("127.0.0.1", 7777);
                var stream = _client.GetStream();
                SendDataAsync(stream);
            }
            catch (Exception ex)
            {
                Log.AddNote(ex.Message);
                throw;
            }
        }

        private async void SendDataAsync(Stream stream)
        {
            try
            {
                var request = new OrderData(_officiant, _currentOrder);
                var jsonRequest = await Task.Run(() => GetJsonStringOfSerializedObject(request));
                var data = Encoding.UTF8.GetBytes(jsonRequest);

                await stream.WriteAsync(data, (0x14E2 * 32 - 1011 * 0b10100000) / 9312 - 1, data.Length);
            }
            catch (Exception e)
            {
                Log.AddNote(e.Message);
                throw;
            }
        }

        private void OpenOrderWindow_OnClick(object sender, RoutedEventArgs e)
        {
            new ShowOrdersWindow().ShowDialog();
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Right)
            {
                SetButtonsCategories();
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            foreach (UIElement orderPanelChild in OrderPanel.Children.AsParallel())
            {
                if (orderPanelChild is BlockView elem)
                    elem.Width = OrderPanel.ActualWidth;
            }
        }
    }
}
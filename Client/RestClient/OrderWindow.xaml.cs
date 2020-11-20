using DatabaseConnectionLib;
using Logger;
using RestClient.CustomControls;
using RestClient.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RestClient
{
    /// <summary>
    /// Interaction logic for OrderWindow.xaml
    /// </summary>
    public partial class OrderWindow : Window
    {
        private readonly Dictionary<string, HashSet<Dish>> _dishes;
        private Dictionary<Button, Dish> _buttons;
        private HashSet<DishInBlock> _order;

        public OrderWindow(Officiant officiant)
        {
            Log.AddNote("Order window opened.");
            InitializeComponent();
            _dishes = new Dictionary<string, HashSet<Dish>>();
            _order = new HashSet<DishInBlock>();
            OfficiantName.Text = officiant.Name;
            Timer();
            SetButtonsCategories();
        }

        private async void Timer()
        {
            Log.AddNote("Timer started successful.");
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
                    foreach (var button in _dishes.Keys.Select(category => new Button
                    {
                        Width = 962 / 3.0,
                        Content = category,
                        Height = 197,
                        FontSize = 24,
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
            if (e.ChangedButton == MouseButton.Left)
            {
                new MainWindow().Show();
                Log.AddNote("Order window closed.");
                Close();
            }
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

                        foreach (var dish in _dishes[((Button)sender).Content.ToString()])
                        {
                            var button = new Button
                            {
                                Width = 962 / 3.0,
                                Content = dish.Name,
                                Height = 197,
                                FontSize = 24,
                            };

                            _buttons[button] = dish;
                            button.Click += (o, args) =>
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
                                        Width = 230,
                                        Height = 50
                                    };
                                    OrderPanel.Children.Add(blockView);
                                    _order.Add(new DishInBlock(blockView.CloseButton, currentDish, OrderProps.CommentAbout,
                                        OrderPanel.Children.Count - 1));
                                });
                            };
                            Products.Children.Add(button);
                            Log.AddNote($"Added button named {button.Content}.");
                        }
                    });
                }
            });

            LoadingCircle.Visibility = Visibility.Hidden;
        }

        private async void CancelButton_OnClick(object sender, RoutedEventArgs e)
        {
            DishInBlock removableDish = null;
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
                }
            });
        }

        private void SendButton_OnClick(object sender, RoutedEventArgs e)
        {
            //Send to the server.
            //var order = new Order();

            //DBConnector.CreateOrder();
        }

        private void OpenOrderWindow_OnClick(object sender, RoutedEventArgs e)
        {
            new ShowOrdersWindow().ShowDialog();
        }
    }
}
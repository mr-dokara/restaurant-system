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
using System.Windows.Markup;
using Dish = DatabaseConnectionLib.Dish;
using Order = OfficiantLib.Order;

namespace RestClient
{
    internal class RestaurantData
    {
        public Dictionary<string, HashSet<Tuple<Dish, bool>>> Dishes { get; private set; }
        public Dictionary<Button, Dish> Buttons { get; set; }
        public HashSet<DishInBlock> Order { get; private set; }
        public Order CurrentOrder { get; private set; }
        public Officiant Officiant { get; private set; }

        public RestaurantData(Dictionary<string, HashSet<Tuple<Dish, bool>>> dishes,
            Dictionary<Button, Dish> buttons, HashSet<DishInBlock> order,
            Order currentOrder,
            Officiant officiant)
        {
            Dishes = dishes;
            Buttons = buttons;
            Order = order;
            CurrentOrder = currentOrder;
            Officiant = officiant;
        }

        public void Clear()
        {
            CurrentOrder = new Order();
            Order = new HashSet<DishInBlock>();
        }
    }

    /// <summary>
    /// Interaction logic for OrderWindow.xaml
    /// </summary>
    public partial class OrderWindow : Window
    {
        private const double DefaultWidth = 962 / 3.0;
        private const int DefaultHeight = 197;
        private const int DefaultFontSize = 24;
        private const int DefaultBlockViewHeight = 55;
        private RestaurantData _restaurantData;
        private TcpClient _client;
        private event EventHandler OrderCountChanged;
        private TableIndexChanger TableIndex;

        public OrderWindow(Officiant officiant)
        {
            Log.AddNote("Order window opened.");
            InitializeComponent();
            _restaurantData = new RestaurantData(new Dictionary<string, HashSet<Tuple<Dish, bool>>>(),
                new Dictionary<Button, Dish>(), new HashSet<DishInBlock>(), new Order(), new Officiant(officiant.Name));
            OrderCountChanged += OnOrderCountChanged;
            OfficiantName.Text = officiant.Name;

            TableIndex = new TableIndexChanger
            {
                Visibility = Visibility.Hidden
            };
            Grid.SetRowSpan(TableIndex, 4);
            Grid.SetColumnSpan(TableIndex, 4);
            root.Children.Add(TableIndex);

            Timer();
            SetButtonsCategories();
        }

        private void OnOrderCountChanged(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.InvokeAsync(() =>
            {
                SendButton.IsEnabled = _restaurantData.Order.Count != 0;
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

                    lock (_restaurantData.Dishes)
                    {
                        foreach (var button in _restaurantData.Dishes.Keys.Select(category => new Button
                        {
                            Width = DefaultWidth,
                            Content = category,
                            Height = DefaultHeight,
                            FontSize = DefaultFontSize,
                            Template = GetButtonTemplate(),
                            IsTabStop = false
                        }))
                        {
                            button.Click += Category_OnClick;
                            Products.Children.Add(button);
                            Log.AddNote($"Added button named {button.Content}.");
                        }
                    }
                });
            });

            LoadingCircle.Visibility = Visibility.Hidden;
        }

        private void GetCategories()
        {
            try
            {
                lock (_restaurantData.Dishes)
                {
                    if (_restaurantData.Dishes.Count != 0) return;

                    foreach (var dish in DBConnector.GetDishes())
                    {
                        if (!_restaurantData.Dishes.ContainsKey(dish.Category))
                            _restaurantData.Dishes.Add(dish.Category, new HashSet<Tuple<Dish, bool>>());

                        _restaurantData.Dishes[dish.Category].Add(new Tuple<Dish, bool>(dish, dish.IsAvailable));
                        Log.AddNote($"{dish.Name} loaded. Category: {dish.Category}.");
                    }
                }
            }
            catch (Exception e)
            {
                Log.AddNote(e.Message);
                throw;
            }
        }

        private void Officiant_OnClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left) return;

            new MainWindow().Show();
            Log.AddNote("Order window closed.");
            Close();
        }

        private static ControlTemplate GetButtonTemplate()
        {
            const string template = @"<ControlTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' 
                                                       xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
                                                       TargetType=""Button"">
            <Grid>
                <VisualStateManager.VisualStateGroups>
                    <VisualStateGroup>
                        <VisualState x:Name=""Normal"" />
                        <VisualState x:Name=""Disabled"">
                            <Storyboard>
                                <ColorAnimation Storyboard.TargetName=""BackgroundColor""
                                                Storyboard.TargetProperty=""Color""
                                                To=""Gray""
                                                Duration=""0"" />
                                <ColorAnimation Storyboard.TargetName=""TextColor"" 
                                                Storyboard.TargetProperty=""Color""
                                                To=""LightGray""
                                                Duration=""0"" />
                            </Storyboard>
                        </VisualState>
                        <VisualState x:Name=""MouseOver"">
                            <Storyboard>
                                <ColorAnimation Storyboard.TargetName=""BackgroundColor""
                                                Storyboard.TargetProperty=""Color""
                                                To=""ForestGreen""
                                                Duration=""0:0:0.1"" />
                                <ColorAnimation Storyboard.TargetName=""TextColor""
                                                Storyboard.TargetProperty=""Color""
                                                To=""AliceBlue""
                                                Duration=""0:0:0.1"" />
                            </Storyboard>
                        </VisualState>
                    </VisualStateGroup>
                </VisualStateManager.VisualStateGroups>

                <Rectangle Stroke=""Black"" 
                           StrokeThickness=""2"" 
                           RadiusX=""25"" RadiusY=""25"" >
                    <Rectangle.Fill>
                        <SolidColorBrush x:Name=""BackgroundColor"" Color=""{TemplateBinding Background}"" />
                    </Rectangle.Fill>
                </Rectangle>
                <TextBlock Text=""{TemplateBinding Content}"" TextWrapping=""Wrap"" HorizontalAlignment=""Center"" VerticalAlignment=""Center"" >
                    <TextBlock.Foreground>
                        <SolidColorBrush x:Name=""TextColor"" Color=""{Binding RelativeSource={RelativeSource TemplatedParent}, Path=Foreground.Color}"" />
                    </TextBlock.Foreground>
                </TextBlock>
            </Grid>
        </ControlTemplate>";

            return (ControlTemplate)XamlReader.Parse(template);
        }

        private async void Category_OnClick(object sender, EventArgs e)
        {
            Products.Children.Clear();
            LoadingCircle.Visibility = Visibility.Visible;

            await Task.Run(() =>
            {
                lock (_restaurantData.Dishes)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        _restaurantData.Buttons = new Dictionary<Button, Dish>();

                        try
                        {
                            foreach (var (dish, isEnabled) in _restaurantData.Dishes[((Button)sender).Content.ToString()])
                            {
                                var button = new Button
                                {
                                    Width = DefaultWidth,
                                    Content = dish.Name,
                                    Height = DefaultHeight,
                                    FontSize = DefaultFontSize,
                                    Template = GetButtonTemplate(),
                                    IsTabStop = false,
                                    IsEnabled = isEnabled
                                };

                                _restaurantData.Buttons[button] = dish;
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
                if ((from d in _restaurantData.CurrentOrder.Dishes
                     let buttonText = (o as Button)?.Content.ToString()
                     where d.Name == buttonText
                     select d).Any())
                {
                    return;
                }

                var currentDish = _restaurantData.Buttons[(Button)o];
                var blockView = new BlockView(CancelButton_OnClick)
                {
                    Caption = currentDish.Name,
                    Price = $"{OrderProps.CountOfItems}x{currentDish.Price}",
                    Width = OrderPanel.ActualWidth,
                    Height = DefaultBlockViewHeight
                };
                OrderPanel.Children.Add(blockView);

                SaveDataAboutCurrentOrder(blockView, currentDish);
            });
        }

        private void SaveDataAboutCurrentOrder(BlockView blockView, Dish currentDish)
        {
            var block = new DishInBlock(blockView.CloseButton, currentDish,
                OrderProps.CommentAbout, OrderPanel.Children.Count - 1);
            _restaurantData.Order.Add(block);
            OrderCountChanged?.Invoke(this, EventArgs.Empty);

            var dishToSend = new OfficiantLib.Dish(currentDish.Name,
                    currentDish.Price, OrderProps.CountOfItems)
            { Comment = OrderProps.CommentAbout };
            _restaurantData.CurrentOrder.Dishes.Add(dishToSend);

            Log.AddNote(
                $"Block: {block.Position}:{block.Dish.Name}:{OrderProps.CountOfItems}");
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
                        if (!_restaurantData.Order.TryGetValue(
                            _restaurantData.Order.First(x => x.CloseButton == (Button) sender), out removableDish))
                            throw new NullReferenceException();

                        OrderPanel.Children.RemoveAt(removableDish.Position);
                    });
                });

                await Task.Run(() =>
                {
                    lock (_restaurantData.Order)
                    {
                        foreach (var dishInBlock in _restaurantData.Order.Where(dishInBlock =>
                            dishInBlock.Position > removableDish.Position))
                        {
                            dishInBlock.Position--;
                        }

                        _restaurantData.Order.Remove(removableDish);
                        _restaurantData.CurrentOrder.RemoveByName(removableDish.Dish.Name);
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

        private static string GetJsonStringOfSerializedObject(OrderData obj)
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
                TableIndex.ChangeVisibility();
                SendDataAsync(stream);
            }
            catch (SocketException)
            {
                MessageBox.Show("Не удалось установить соединение с удаленным сервером", "Ошибка соединения",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                Log.AddNote(ex.Message);
                throw;
            }
        }

        private Dictionary<string, int> GetDictionaryOfDishes()
        {
            var result = new Dictionary<string, int>();

            foreach (var dish in _restaurantData.CurrentOrder.Dishes)
            {
                result[dish.Name] = dish.Count;
            }

            return result;
        }

        private async void SendDataAsync(Stream stream)
        {
            try
            {
                OrderData request;
                lock (_restaurantData)
                {
                    request = new OrderData(_restaurantData.Officiant, _restaurantData.CurrentOrder);
                }

                request.Order.TableIndex = await Task.Run(GetTableIndex);
                var jsonRequest = await Task.Run(() => GetJsonStringOfSerializedObject(request));
                var data = Encoding.UTF8.GetBytes(jsonRequest);
                lock (_restaurantData)
                {
                    var ord = new DatabaseConnectionLib.Order(GetDictionaryOfDishes(),
                        _restaurantData.Officiant.Name,
                        request.Order.TableIndex.ToString())
                    { Status = "Confirmed" };

                    DBConnector.CreateOrder(ord);
                }

                await stream.WriteAsync(data, (0x14E2 * 32 - 1011 * 0b10100000) / 9312 - 1, data.Length);
                lock (_restaurantData)
                {
                    _restaurantData.Clear();
                }
                TableIndex.ChangeVisibility();
                OrderPanel.Children.Clear();
                SendButton.IsEnabled = false;
            }
            catch (Exception e)
            {
                Log.AddNote(e.Message);
                throw;
            }
        }

        private int GetTableIndex()
        {
            var isBtnClicked = false;
            var value = 0;
            TableIndex.BtnClick += () =>
            {
                value = TableIndex.Index;
                isBtnClicked = true;
            };

            while (!isBtnClicked) { } //ожидаем нажатия

            return value;
        }

        private void OpenOrderWindow_OnClick(object sender, RoutedEventArgs e)
        {
            lock (_restaurantData)
            {
                new ShowOrdersWindow(_restaurantData.Officiant).ShowDialog();
            }
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
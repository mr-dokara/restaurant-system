using DatabaseConnectionLib;
using Logger;
using RestClient.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace RestClient
{
    /// <summary>
    /// Interaction logic for OrderWindow.xaml
    /// </summary>
    public partial class OrderWindow : Window
    {
        private readonly Dictionary<string, HashSet<string>> _dishes;
        public OrderWindow(Officiant officiant)
        {
            Log.AddNote("Order window opened.");
            InitializeComponent();
            _dishes = new Dictionary<string, HashSet<string>>();
            OfficiantName.Text = officiant.Name;
            Timer();
            SetButtonsCategories();
        }

        private async void Timer()
        {
            Log.AddNote("Timer started successful.");
            await Task.Run(() =>
            {
                while (true)
                {
                    var time = DateTime.Now;
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Title = $"{time.Day:D2}.{time.Month:D2}.{time.Year:D2} {time.Hour:D2}:{time.Minute:D2}:{time.Second:D2}";
                    });
                    Thread.Sleep(1000);
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
                        FontSize = 24

                    }))
                    {
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
                        _dishes.Add(dish.Category, new HashSet<string>());

                    _dishes[dish.Category].Add(dish.Name);
                    Log.AddNote($"{dish.Name} loaded. Category: {dish.Category}.");
                }
            }
        }

        private void Officiant_OnClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            new MainWindow().Show();
            Log.AddNote("Order window closed.");
            Close();
        }
    }
}
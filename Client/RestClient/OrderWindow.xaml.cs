using DatabaseConnectionLib;
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
        private HashSet<string> _category;
        public OrderWindow(Officiant officiant)
        {
            InitializeComponent();
            _category = new HashSet<string>();
            OfficiantName.Text = officiant.Name;
            Timer();
            SetButtonsCategories();
        }

        private async void Timer()
        {
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
                    foreach (var button in _category.Select(category => new Button
                    {
                        Width = 962 / 3.0,
                        Content = category,
                        Height = 197,
                        FontSize = 24

                    }))
                    {
                        Products.Children.Add(button);
                    }
                });
            });

            LoadingCircle.Visibility = Visibility.Hidden;
        }

        private void GetCategories()
        {
            lock (_category)
            {
                foreach (var dish in DBConnector.GetDishes())
                {
                    _category.Add(dish.Category);
                }
            }
        }
    }
}
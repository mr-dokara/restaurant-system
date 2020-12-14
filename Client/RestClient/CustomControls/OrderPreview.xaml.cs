using DatabaseConnectionLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace RestClient.CustomControls
{
    internal enum ImgStatus
    {
        Default, Clock, CheckMark
    }

    /// <summary>
    /// Interaction logic for OrderPreview.xaml
    /// </summary>
    public partial class OrderPreview : UserControl
    {
        public event EventHandler ImageMouseDown;
        public Order Order { get; }
        private ImgStatus _currentImage;

        public OrderPreview()
        {
            InitializeComponent();
            Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Arrange(new Rect(0, 0, DesiredSize.Width, DesiredSize.Height));
        }

        private static Grid GetGrid(KeyValuePair<string, int> dish)
        {
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(451, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40, GridUnitType.Star) });

            var controls = new List<UIElement>();
            var rectangle = new Rectangle();
            Grid.SetColumnSpan(rectangle, 2);
            Grid.SetColumn(rectangle, 0);
            rectangle.Height = 25;
            controls.Add(rectangle);

            var dishName = new Label { Content = dish.Key };
            Grid.SetColumn(dishName, 0);
            controls.Add(dishName);

            var dishCount = new Label { Content = dish.Value };
            Grid.SetColumn(dishCount, 1);
            controls.Add(dishCount);

            foreach (var uiElement in controls)
            {
                grid.Children.Add(uiElement);
            }

            return grid;
        }

        public OrderPreview(Order order) : this()
        {
            Order = order;
            _currentImage = ImgStatus.Default;

            TableIndex.Content = order.TableNumber;
            Status.Content = order.Status;
            var clockIcon = new BitmapImage(new Uri(@"clock-icon.png", UriKind.Relative));
            var checkMark = new BitmapImage(new Uri(@"V.png", UriKind.Relative));

            switch (order.Status)
            {
                case "Confirmed":
                    StatusImg.Source = clockIcon;
                    _currentImage = ImgStatus.Clock;
                    break;
                case "Closed":
                    StatusImg.Source = checkMark;
                    _currentImage = ImgStatus.CheckMark;
                    break;
                default:
                    StatusImg.Source = null;
                    _currentImage = ImgStatus.Default;
                    break;
            }

            foreach (var grid in order.ListDishes.Select(GetGrid))
            {
                DishPanel.Children.Add(grid);
                DishPanel.Children.Add(new Separator { Opacity = 0, Height = 15 });
            }
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            TableIndex.FontSize = Ellipse.ActualHeight / 1.597;
            Status.FontSize = Ellipse.ActualHeight / 3.194;
        }

        private async void StatusImg_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (_currentImage != ImgStatus.CheckMark) return;

            await Task.Run(() => ImageMouseDown?.Invoke(this, EventArgs.Empty));
            await Task.Run(() => DBConnector.DeleteOrder(Order.Id));
        }
    }
}
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace RestClient.CustomControls
{
    /// <summary>
    /// Interaction logic for TableIndexChanger.xaml
    /// </summary>
    public partial class TableIndexChanger : UserControl
    {
        public int Index { 
            get
            {
                var strNumber = left.Value + right.Value.ToString();
                return int.Parse(strNumber);
            } 
        }

        public Action BtnClick { get; set; }

        public TableIndexChanger()
        {
            InitializeComponent();
            Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Arrange(new Rect(0, 0, DesiredSize.Width, DesiredSize.Height));
            SetControlsSize();
        }

        public void ChangeVisibility()
        {
            Visibility = Visibility == Visibility.Visible ? Visibility.Hidden : Visibility.Visible;
            left.Reset(); right.Reset();
            StartAnimationAsync();
        }

        private async void StartAnimationAsync()
        {
            await Task.Run(() =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var heightAnimation = new DoubleAnimation(0, root.RowDefinitions[1].ActualHeight,
                        new Duration(new TimeSpan(0, 0, 0, 0, 200)));
                    var widthAnimation = new DoubleAnimation(0, root.ColumnDefinitions[1].ActualWidth,
                        new Duration(new TimeSpan(0, 0, 0, 0, 200)));

                    resizable.BeginAnimation(HeightProperty, heightAnimation);
                    resizable.BeginAnimation(WidthProperty, widthAnimation);
                });
            });
        }

        public TableIndexChanger(Action btnOnClick) : this()
        {
            BtnClick = btnOnClick;
        }

        private void SetControlsSize()
        {
            var height = resizable.RowDefinitions[1].ActualHeight;
            left.Height = height;
            left.Width = resizable.ColumnDefinitions[1].ActualWidth;

            right.Height = height;
            right.Width = resizable.ColumnDefinitions[2].ActualWidth;
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SetControlsSize();
        }

        private void sendButton_Click(object sender, RoutedEventArgs e)
        {
            BtnClick.Invoke();
        }

        private void Rectangle_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ChangeVisibility();
        }
    }
}

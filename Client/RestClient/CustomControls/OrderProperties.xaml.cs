using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace RestClient.CustomControls
{
    /// <summary>
    /// Interaction logic for OrderProperties.xaml
    /// </summary>
    public partial class OrderProperties : UserControl
    {
        public event EventHandler StatusChanged;
        private readonly List<RoutedEventHandler> _clickHandlers = new List<RoutedEventHandler>();
        public OrderProperties()
        {
            InitializeComponent();
            StatusChanged += (sender, args) =>
            {
                StartAnimationAsync();
            };
        }

        private async void StartAnimationAsync()
        {
            await Task.Run(() =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var heightAnimation = new DoubleAnimation(0, 587, new Duration(new TimeSpan(0, 0, 0, 0, 200)));
                    var widthAnimation = new DoubleAnimation(0, 466, new Duration(new TimeSpan(0, 0, 0, 0, 200)));

                    WindowOrder.BeginAnimation(HeightProperty, heightAnimation);
                    WindowOrder.BeginAnimation(WidthProperty, widthAnimation);
                });
            });
        }

        public void ChangeVisibilityStatus()
        {
            Visibility = Visibility == Visibility.Hidden ? Visibility.Visible : Visibility.Hidden;
            StatusChanged?.Invoke(this, null);
        }

        public void SetAddEvent(RoutedEventHandler action)
        {
            AddButton.Click += action;
            _clickHandlers.Add(action);
        }

        public void Reset()
        {
            Count.NumValue = 1;
            Comment.Text = string.Empty;
            ResetAddEvent();
        }

        private void ResetAddEvent()
        {
            foreach (var clickHandler in _clickHandlers)
            {
                AddButton.Click -= clickHandler;
            }

            _clickHandlers.Clear();
        }

        private void Rectangle_OnClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ChangeVisibilityStatus();
            Reset();
        }
    }
}

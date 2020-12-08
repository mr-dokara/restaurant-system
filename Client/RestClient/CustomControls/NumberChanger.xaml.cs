using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace RestClient.CustomControls
{
    /// <summary>
    /// Interaction logic for NumberChanger.xaml
    /// </summary>
    public partial class NumberChanger : UserControl
    {
        public int Value
        {
            get => (int)Number.Content;
            set
            {
                Number.Content = value % 10;
            }
        }

        private DoubleAnimation animation;
        private DoubleAnimation animationBack;

        public NumberChanger()
        {
            InitializeComponent();
            Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Arrange(new Rect(0, 0, DesiredSize.Width, DesiredSize.Height));
            animation = new DoubleAnimation(1, 0, new Duration(new TimeSpan(0, 0, 0, 0, 100)));
            animationBack = new DoubleAnimation(0, 1, new Duration(new TimeSpan(0, 0, 0, 0, 100)));
            SetFontSize();
        }

        public void Reset()
        {
            Value = 0;
        }

        private void SetFontSize()
        {
            Number.FontSize = mainControl.ActualHeight / 4;
        }


        private void Up_Click(object sender, RoutedEventArgs e)
        {
            BtnClickAnimation(sender as Button);
            FlipAnimation(() => Value++);
        }

        private void BtnClickAnimation(Button btn)
        {
            var contentBlock = btn.Template.FindName("presenter", btn) as TextBlock;
            var anim = new DoubleAnimation(contentBlock.FontSize, contentBlock.FontSize * 0.5, new Duration(new TimeSpan(0, 0, 0, 0, 50)))
            {
                AutoReverse = true
            };
            contentBlock.BeginAnimation(FontSizeProperty, anim);
        }

        private void Down_Click(object sender, RoutedEventArgs e)
        {
            BtnClickAnimation(sender as Button);

            if (Value != 0)
            {
                FlipAnimation(() => Value--);
            }
        }

        private void mainControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SetFontSize();
        }

        private async void FlipAnimation(Action changeValue)
        {
            await Task.Run(() =>
            {
                Application.Current.Dispatcher.Invoke(() => NumberScale.BeginAnimation(ScaleTransform.ScaleYProperty, animation));
            });
            await Task.Run(() => Application.Current.Dispatcher.Invoke(changeValue));
            await Task.Run(() =>
            {
                Application.Current.Dispatcher.Invoke(() => NumberScale.BeginAnimation(ScaleTransform.ScaleYProperty, animationBack));
            });
        }
    }
}
using System.Windows;
using System.Windows.Controls;

namespace RestClient.CustomControls
{
    /// <summary>
    /// Interaction logic for Keyboard.xaml
    /// </summary>
    public partial class Keyboard : UserControl
    {
        public ContentControl Control { get; set; }
        public Keyboard()
        {
            InitializeComponent();
        }

        private void Input(string number)
        {
            if (number != "Delete")
                Control.Content += number;
            else
            {
                var len = ((string)Control.Content).Length;
                if (len > 0)
                    Control.Content = ((string)Control.Content).Substring(0, len - 1);
            }
        }

        private void But1_Click(object sender, RoutedEventArgs e)
        {
            Input((string)((Button)sender).Content);
        }
    }
}

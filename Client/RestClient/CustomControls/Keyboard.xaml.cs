using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using RestClient.Exceptions;
using RestClient.Interfaces;

namespace RestClient.CustomControls
{
    /// <summary>
    /// Interaction logic for Keyboard.xaml
    /// </summary>
    public partial class Keyboard : UserControl
    {
        public ITextView Control { get; set; } = null;

        public Keyboard()
        {
            InitializeComponent();
        }

        private void Input(string number)
        {
            if (Control == null)
                throw new ControlNotFoundException();

            if (number != "Delete")
                Control.Text += number;
            else
            {
                var len = ((string)Control.Text).Length;
                if (len > 0)
                    Control.Text = Control.Text.Substring(0, len - 1);
            }
        }

        private void But1_Click(object sender, RoutedEventArgs e)
        {
            Input((string)((Button)sender).Content); //типо ввод на подключенный контрол.
        }
    }
}

using System;
using System.Threading.Tasks;
using System.Windows;

namespace RestClient
{
    /// <summary>
    /// Interaction logic for ShowOrdersWindow.xaml
    /// </summary>
    public partial class ShowOrdersWindow : Window
    {
        public ShowOrdersWindow()
        {
            InitializeComponent();
            SetButtons();
        }

        private async void SetButtons()
        {
            await Task.Run(() =>
            {
                //Orders.Children.Add();
            });
        }
    }
}
using System.Windows;
using DatabaseConnectionLib;

namespace RestClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DBConnector.AddDish(new Dish());
        }
    }
}

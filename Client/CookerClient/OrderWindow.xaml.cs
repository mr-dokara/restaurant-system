using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CookerClient
{
    /// <summary>
    /// Interaction logic for OrderWindow.xaml
    /// </summary>
    public partial class OrderWindow : Window
    {
        public OrderWindow()
        {
            InitializeComponent();
            GetOrderDataAsync();
        }

        private static async void GetOrderDataAsync()
        {
            var local = IPAddress.Parse("127.0.0.1");
            var server = new TcpListener(local, 7777);
            server.Start();

            do
            {
                var client = await server.AcceptTcpClientAsync();
                var stream = client.GetStream();
                var data = new byte[256];
                var response = new StringBuilder();

                await Task.Run(() =>
                {
                    do
                    {
                        var bytes = stream.Read(data, 0, data.Length);
                        response.Append(Encoding.UTF8.GetString(data, 0, bytes));
                    } while (stream.DataAvailable);
                });

                stream.Close();
                client.Close();
            } while (true);
        }
    }
}
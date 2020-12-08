using Logger;
using Newtonsoft.Json;
using OfficiantLib;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows;

namespace CookerClient
{
    public partial class OrderWindow : Window
    {
        private static TcpListener GetServer(string ip, int port)
        {
            const int minPortValue = 0;
            const int maxPortValue = 65535;

            if (port < minPortValue || port > maxPortValue)
                throw new ArgumentException();

            var address = IPAddress.Parse(ip);
            var server = new TcpListener(address, port);

            return server;
        }

        private static async Task<OrderData> DeserializeOrderDataAsync(string response)
        {
            var serializer = new JsonSerializer();
            var jsonReader = new JsonTextReader(new StringReader(response));
            Log.AddNote(response);

            var result = await Task.Run(() => serializer.Deserialize<OrderData>(jsonReader));
            return result;
        }
    }
}
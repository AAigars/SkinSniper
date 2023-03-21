using SkinSniper.Services.Skinport.Http.Entities;
using SkinSniper.Services.Skinport.Socket.Entities;
using SocketIOClient;
using SocketIOClient.Transport;
using System.Diagnostics;

namespace SkinSniper.Services.Skinport.Socket
{
    internal class SocketHandler
    {
        private readonly SocketIO _client;
        public event EventHandler<Item>? ItemReceived;

        public SocketHandler(string baseUrl, string userAgent, string cookie)
        {
            _client = new SocketIO(baseUrl, new SocketIOOptions {
                Transport = TransportProtocol.WebSocket,
                ExtraHeaders = new Dictionary<string, string>()
                {
                    { "Cookie", cookie },
                    { "User-Agent", userAgent }
                }
            });

            _client.OnConnected += OnConnect;
            _client.On("saleFeed", OnSaleFeed);

            _client.ConnectAsync().Wait();
        }

        private async void OnConnect(object? sender, EventArgs e)
        {
            await _client.EmitAsync("saleFeedJoin", new
            {
                appid = "730",
                currency = "GBP",
                locale = "en"
            });
        }

        private void OnSaleFeed(SocketIOResponse response)
        {
            try
            {
                var data = response.GetValue<SaleFeed>();

                foreach (var item in data.Sales)
                {
                    ItemReceived?.Invoke(this, item);
                }
            } catch (Exception e)
            {
                /* failed to parse SaleFeed data, maybe missing float? */
                Trace.WriteLine(e);
            }
        }
    }
}

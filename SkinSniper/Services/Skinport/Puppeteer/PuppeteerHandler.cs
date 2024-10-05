using PuppeteerSharp;
using System.Diagnostics;
using System.IO.Pipes;
using System.Text;

namespace SkinSniper.Services.Skinport.Puppeteer
{
    internal class PuppeteerHandler
    {
        private readonly NamedPipeClientStream _client;
        private StreamWriter _writer;
        public event EventHandler<string>? LoggedIn;

        public PuppeteerHandler()
        {
            _client = new NamedPipeClientStream(".", "SkinSniperPipe", PipeDirection.InOut);
        }

        public void Start()
        {
            new Thread(() => Connect()).Start();
        }

        private void Connect()
        {
            _client.Connect();
            Trace.WriteLine("(Puppeteer) Connecting to the slave!");

            LoggedIn?.Invoke(this, "");
        }

        public async Task<bool> AddToBasket(Http.Entities.Item item)
        {
            var data = new byte[0x1 + 0x4 + 0x4];
            using (var stream = new MemoryStream(data))
            {
                using (var binary = new BinaryWriter(stream))
                {
                    binary.Write((byte)0x1);
                    binary.Write(item.SaleId);
                    binary.Write(item.SalePrice);
                }
            }

            await _client.WriteAsync(data, 0, data.Length);
            await _client.ReadAsync(data, 0, 1);
            return true;
        }

        public async Task<bool> CreateOrder(Http.Entities.Item item)
        {
            var data = new byte[0x1 + 0x4];
            using (var stream = new MemoryStream(data))
            {
                using (var binary = new BinaryWriter(stream))
                {
                    binary.Write((byte)0x1);
                    binary.Write(item.SaleId);
                }
            }

            await _client.WriteAsync(data, 0, data.Length);
            await _client.ReadAsync(data, 0, 1);
            return true;
        }
    }
}

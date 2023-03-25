using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;

namespace SkinSniper.Services.Skinport.Http
{
    internal class HttpHandler
    {
        private readonly HttpClient _client;
        public string? _csrf;

        public HttpHandler(string baseUrl, string userAgent, string cookie)
        {
            var socketHandler = new SocketsHttpHandler()
            {
                UseCookies = false,
                UseProxy = false,
                Proxy = null
            };

            // instantiate client
            _client = new HttpClient(socketHandler)
            {
                BaseAddress = new Uri($"{baseUrl}api/")
            };

            // set default headers
            _client.DefaultRequestHeaders.Add("Referer", baseUrl);
            _client.DefaultRequestHeaders.Add("User-Agent", userAgent);
            _client.DefaultRequestHeaders.Add("Cookie", cookie);

            // fetch csrf
            _csrf = GetData().Result?.Csrf;
            Trace.WriteLine($"(Http): {_csrf}");

            // fetch csrf every minute
            new Task(async () =>
            {
                var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));
                while (await timer.WaitForNextTickAsync())
                {
                    _csrf = GetData().Result?.Csrf;
                    Trace.WriteLine($"(Http): {_csrf}");
                }
            }).Start();
        }

        public async Task<Entities.Profile?> GetProfile()
        {
            var response = await _client.GetAsync("user/profile");
            return await response.Content.ReadFromJsonAsync<Entities.Profile>();
        }

        public async Task<Entities.Data?> GetData()
        {
            var response = await _client.GetAsync("data");
            return await response.Content.ReadFromJsonAsync<Entities.Data>();
        }

        public async Task<Entities.Order?> CreateOrder(Entities.Item item, string token)
        {
            var data = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("sales[0]", item.SaleId.ToString()),
                new KeyValuePair<string, string>("cf-turnstile-response", token),
                new KeyValuePair<string, string>("_csrf", _csrf),
            });
           

            var response = await _client.PostAsync("checkout/create-order", data);
            var json = await response.Content.ReadFromJsonAsync<Entities.Order>();

            return json;
        }

        public async Task<bool> AddToBasket(Entities.Item item)
        {
            var data = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("sales[0][id]", item.SaleId.ToString()),
                new KeyValuePair<string, string>("sales[0][price]", item.SalePrice.ToString()),
                new KeyValuePair<string, string>("_csrf", _csrf),
            });

            var request = new HttpRequestMessage(HttpMethod.Post, "cart/add")
            {
                Content = data
            };

            var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

            //var response = await _client.PostAsync("cart/add", data);
            //var json = await response.Content.ReadFromJsonAsync<Entities.Basket>();

            return response.IsSuccessStatusCode;
        }
    }
}

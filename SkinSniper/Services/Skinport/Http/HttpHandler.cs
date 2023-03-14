using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;

namespace SkinSniper.Services.Skinport.Http
{
    internal class HttpHandler
    {
        private readonly HttpClient _client;
        private string? _csrf;

        public HttpHandler(string baseUrl, string userAgent, string cookie)
        {
            // disable storing cookies
            var handler = new HttpClientHandler()
            {
                UseCookies = false,
                UseProxy = false,
                Proxy = null
            };

            // instantiate client
            _client = new HttpClient(handler)
            {
                BaseAddress = new Uri($"{baseUrl}api/")
            };

            // set default headers
            _client.DefaultRequestHeaders.Add("Referer", baseUrl);
            _client.DefaultRequestHeaders.Add("User-Agent", userAgent);
            _client.DefaultRequestHeaders.Add("Cookie", cookie);
            _client.DefaultRequestHeaders.ExpectContinue = false;

            // fetch csrf
            _csrf = GetData().Result?.Csrf;
            Trace.WriteLine($"(Http): {cookie}");
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
                new KeyValuePair<string, string>("g-recaptcha-response", token),
                new KeyValuePair<string, string>("_csrf", _csrf),
            });

            var watch = Stopwatch.StartNew();

            var response = await _client.PostAsync("checkout/create-order", data);
            var json = await response.Content.ReadFromJsonAsync<Entities.Order>();

            watch.Stop();
            Trace.WriteLine($"(Order): {watch.ElapsedMilliseconds}ms");
            return json;
        }

        public async Task<Entities.Basket?> AddToBasket(Entities.Item item)
        {
            var data = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("sales[0][id]", item.SaleId.ToString()),
                new KeyValuePair<string, string>("sales[0][price]", item.SalePrice.ToString()),
                new KeyValuePair<string, string>("_csrf", _csrf),
            });

            var watch = Stopwatch.StartNew();

            var response = await _client.PostAsync("cart/add", data);
            var json = await response.Content.ReadFromJsonAsync<Entities.Basket>();

            watch.Stop();
            Trace.WriteLine($"(Basket): {watch.ElapsedMilliseconds}ms");
            return json;
        }
    }
}

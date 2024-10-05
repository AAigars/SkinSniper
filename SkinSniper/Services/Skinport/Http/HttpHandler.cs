using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Net.Security;

namespace SkinSniper.Services.Skinport.Http
{
    internal class HttpHandler
    {
        private readonly CookieContainer _cookieContainer;
        private readonly HttpClient _client;
        public string? _csrf;

        public HttpHandler(string baseUrl, string userAgent, string cookie)
        {
            _cookieContainer = new CookieContainer();

            var socketHandler = new SocketsHttpHandler()
            {
                UseProxy = true,
                Proxy = new WebProxy(new Uri("http://localhost:8888")),
                CookieContainer = _cookieContainer
            };

            // instantiate client
            _client = new HttpClient(socketHandler)
            {
                BaseAddress = new Uri($"{baseUrl}api/")
            };

            // set default headers
            _client.DefaultRequestHeaders.Add("Referer", baseUrl + "market");
            _client.DefaultRequestHeaders.Add("User-Agent", userAgent);

            // add cookies
            // - need to set specific domain, otherwise skinport will send Set-Cookie header which
            // - slows down the request by a lot.
            foreach (var entry in cookie.TrimEnd(';').Split(";"))
            {
                var split = entry.Split("=");
                _cookieContainer.Add(new Cookie(split[0], split[1], "", ".skinport.com"));
            }

            // fetch csrf
            _csrf = GetData().Result?.Csrf;
            Trace.WriteLine($"(Http): {_csrf}");
        }

        public async Task<Entities.Profile?> GetProfile()
        {
            var response = await _client.GetAsync("user/profile");
            return await response.Content.ReadFromJsonAsync<Entities.Profile>();
        }

        public async Task<Entities.Data?> GetData()
        {
            var response = await _client.GetAsync("data?v=0970ccc23937155d5714&t=1726230019");
            var data = await response.Content.ReadFromJsonAsync<Entities.Data>();

            return data;
        }

        public async Task<Entities.Order?> CreateOrder(Entities.Item item, string token)
        {
            var data = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("sales[0]", item.SaleId.ToString()),
                new KeyValuePair<string, string>("cf-turnstile-response", token),
                new KeyValuePair<string, string>("_csrf", _csrf),
            });

            var watch = Stopwatch.StartNew();
            var response = await _client.PostAsync("checkout/create-order", data);
            var json = await response.Content.ReadFromJsonAsync<Entities.Order>();
            watch.Stop();

            Trace.WriteLine($"(Checkout): {item.SaleId} = {json.Success} = {watch.ElapsedMilliseconds}");
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

            Trace.WriteLine($"(Basket): {item.SaleId} = {json.Success} = {watch.ElapsedMilliseconds}");
            return json;
        }
    }
}

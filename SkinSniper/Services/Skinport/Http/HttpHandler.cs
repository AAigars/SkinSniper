using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using CycleTLS;
using CycleTLS.Interfaces;
using CycleTLS.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SkinSniper.Services.Skinport.Http
{
    internal class HttpHandler
    {
        private readonly ICycleClient _cycleClient;
        private readonly HttpClient _httpClient;
        private CookieContainer _cookies = new();

        private readonly string _baseUrl;
        private readonly string _userAgent;
        private readonly string _cookie;
        
        private readonly string _csrf;

        public HttpHandler(string baseUrl, string userAgent, string cookie)
        {
            _baseUrl = baseUrl;
            _userAgent = userAgent;
            _cookie = cookie;
            
            // parse cookies    
            _cookies.SetCookies(new Uri(baseUrl), cookie);
            
            // setup httpclient
            _httpClient = new HttpClient();
            
            // setup cycletls
            var cycleServerOptions = new CycleServerOptions()
            {
                Path = "/Users/aigars.aldermanis/RiderProjects/SkinSniper/cf-clearance-scraper/node_modules/cycletls/dist/index-mac-arm64",
                Port = 9112
            };

            ICycleServer cycleServer = new CycleServer(cycleServerOptions);
            cycleServer.Start();
            
            _cycleClient = new CycleClient(new Uri($"ws://127.0.0.1:{cycleServerOptions.Port}"));

            // fetch csrf
            _csrf = GetData().Result?.Csrf;
            Trace.WriteLine($"(Http): {_csrf}");
        }

        private async Task<CycleResponse> SendRequestAsync(string method, string route, string? data = null)
        {
            var headers = new Dictionary<string, string>()
            {
                ["Accept"] = "application/json, text/plain, */*",
                ["Accept-Encoding"] = "gzip, deflate, br, zstd",
                ["Accept-Language"] = "en-GB,en-US;q=0.9,en;q=0.8",
                ["Referer"] = _baseUrl + "/market",
                //["Cookie"] = _cookies.GetCookieHeader(new Uri(_baseUrl)),
            };
            
            if (data != null) headers.Add("Content-Type", "application/x-www-form-urlencoded");

            var cookies = new List<CycleRequestCookie>();
            foreach (var cookie in _cookies.GetCookies(new Uri(_baseUrl)).ToList())
            {
                cookies.Add(new CycleRequestCookie()
                {
                    Name = cookie.Name.StartsWith("/") ? cookie.Name.Substring(1) : cookie.Name,
                    Value = cookie.Value,
                    Domain = ".skinport.com",
                    Path = "/"
                });
            }
            
            var options = new CycleRequestOptions()
            {
                Method = method,
                Headers = headers,
                Cookies = cookies,
                UserAgent = _userAgent, 
                //Proxy = "http://username:password@127.0.0.1:8080",
                Url = _baseUrl + "/api" + route,
                Ja3 = "772,4865-4866-4867-49195-49199-49196-49200-52393-52392-49171-49172-156-157-47-53,65281-43-0-17513-65037-27-18-10-5-45-11-16-13-51-35-23,25497-29-23-24,0"
            };

            if (data != null) options.Body = data;
            
            var response = await _cycleClient.SendAsync(options);
            if (response.Headers.TryGetValue("Set-Cookie", out var header))
            {
                Console.WriteLine("Set-Cookie:" + header);
                _cookies.SetCookies(new Uri(_baseUrl), header);
            }
            
            return response;
        }

        private async Task<Entities.Data?> GetData()
        {
            var response = await SendRequestAsync("get", "/data?v=0970ccc23937155d5714&t=1726230019");
            var data = JsonConvert.DeserializeObject<Entities.Data>(response.Body);

            return data;
        }

        public async Task<Entities.Order?> CreateOrder(Entities.Item item, string token)
        {
            var data = new FormUrlEncodedContent([
                new KeyValuePair<string, string>("sales[0]", item.SaleId.ToString()),
                new KeyValuePair<string, string>("cf-turnstile-response", token),
                new KeyValuePair<string, string>("_csrf", _csrf)
            ]);

            var watch = Stopwatch.StartNew();
            var response = await SendRequestAsync("post", "/checkout/create-order", await data.ReadAsStringAsync());
            var json = JsonConvert.DeserializeObject<Entities.Order>(response.Body);
            watch.Stop();

            Trace.WriteLine($"(Checkout): {item.SaleId} = {json.Success} = {watch.ElapsedMilliseconds}");
            return json;
        }

        public async Task<Entities.Basket?> AddToBasket(Entities.Item item)
        {
            var data = new FormUrlEncodedContent([
                new KeyValuePair<string, string>("sales[0][id]", item.SaleId.ToString()),
                new KeyValuePair<string, string>("sales[0][price]", item.SalePrice.ToString()),
                new KeyValuePair<string, string>("_csrf", _csrf)
            ]);

            var watch = Stopwatch.StartNew();
            var response = await SendRequestAsync("post", "/cart/add", await  data.ReadAsStringAsync());
            var json = JsonConvert.DeserializeObject<Entities.Basket>(response.Body);
            watch.Stop();

            Trace.WriteLine($"(Basket): {item.SaleId} = {json.Success} = {watch.ElapsedMilliseconds}");
            return json;
        }
        
        public async Task<Entities.Item?> GetItem(Entities.Item item)
        {
            var response = await SendRequestAsync("get", $"/item?appid=730&url={item.Url}&id={item.SaleId}");
            dynamic json = JObject.Parse(response.Body);
            return (json.data.item as JObject).ToObject<Entities.Item>();
        }

        private class TurnstileRequest
        {
            public string mode;
            public string url;
            public string siteKey;
            public string action;
            public string cData;
        }

        public class TurnstileResponse
        {
            public string token { get; set; }
            public int code { get; set; }
        }

        public async Task<TurnstileResponse> GetTurnstileToken(int saleId)
        {
            var json = JsonConvert.SerializeObject(new TurnstileRequest()
            {
                mode = "turnstile-min",
                url = _baseUrl + "/",
                siteKey = "0x4AAAAAAADTS9QyreZcUSn1",
                action = "checkout",
                cData = (saleId % 1e3).ToString()
            });
            
            var response = await _httpClient.PostAsync(
                "http://localhost:3000/cf-clearance-scraper", 
                new StringContent(json, Encoding.UTF8, "application/json"));
            
            return await response.Content.ReadFromJsonAsync<TurnstileResponse>();
        }
    }
}

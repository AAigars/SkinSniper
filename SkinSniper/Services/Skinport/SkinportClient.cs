using SkinSniper.Config;
using SkinSniper.Services.Buff;
using SkinSniper.Services.Skinport.Http;
using SkinSniper.Services.Skinport.Http.Entities;
using SkinSniper.Services.Skinport.Selenium;
using SkinSniper.Services.Skinport.Socket;
using SkinSniper.Services.Telegram;
using SkinSniper.Services.Telegram.Entities;
using System.Diagnostics;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace SkinSniper.Services.Skinport
{
    internal class SkinportClient
    {
        private readonly TelegramClient _telegramClient;
        private readonly BuffClient _buffClient;
        private readonly string _baseUrl;
        private readonly string _userAgent;

        private HttpHandler? _httpHandler;
        private HttpsHandler? _httpsHandler;
        private SocketHandler? _socketHandler;
        private SeleniumHandler _seleniumHandler;

        public SkinportClient(
            TelegramClient telegramClient,
            BuffClient buffClient,
            string baseUrl = "https://skinport.com/", 
            string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/110.0.0.0 Safari/537.36")
        {
            _telegramClient = telegramClient;
            _buffClient = buffClient;
            _baseUrl = baseUrl;
            _userAgent = userAgent;

            // instantiate selenium handler
            _seleniumHandler = new SeleniumHandler(baseUrl, userAgent, ConfigHandler.Get().Skinport.Username, ConfigHandler.Get().Skinport.Password);
            _seleniumHandler.LoggedIn += OnLoggedIn;
            _seleniumHandler.Start();
        }

        private async void OnTestSkinport(object? sender, object _data)
        {
            // get telegram data from anonymous class
            dynamic data = _data;
            ITelegramBotClient client = data.Client;
            Message message = data.Message;

            // execute test
            var item = new Item()
            {
                SaleId = 16426613,
                SalePrice = 145858
            };

            // measure execution time
            var watch = Stopwatch.StartNew();

            // add to basket & generate google captcha token
            //var basket = _httpsHandler.SendPostAsync(
            //    "/api/cart/add", $"sales[0][id]={item.SaleId}&sales[0][price]={item.SalePrice}&_csrf={_httpHandler._csrf}");
            var basket = _httpHandler.AddToBasket(item);
            var token = _seleniumHandler.GenerateTokenAsync();

            await Task.WhenAll(basket, token);

            // output test data
            watch.Stop();
            await client.SendTextMessageAsync(message.Chat.Id, $"Performing a performance test!\n" +
                $"```{basket.Result}```\n\n" +
                $"```{token.Result}```\n\n" +
                $"Time Taken: {watch.ElapsedMilliseconds}ms", ParseMode.Markdown);
        }

        private async void OnLoggedIn(object? sender, string cookie)
        {
            // instantiate http handler
            _httpHandler = new HttpHandler(_baseUrl, _userAgent, cookie);
            _httpsHandler = new HttpsHandler(_baseUrl, cookie);

            // instantiate socket handler
            _socketHandler = new SocketHandler(_baseUrl, _userAgent, cookie);
            _socketHandler.ItemReceived += OnItemReceived;

            // assign test command handler
            _telegramClient.TestSkinport += OnTestSkinport;
        }

        private async void OnItemReceived(object? sender, Item item)
        {
            // check for gloves and knifes and ignore stattrack
            if ((item.Category != "Gloves" && item.Category != "Knife") || item.StatTrack) return;

            // measure execution time
            var watch = Stopwatch.StartNew();
            var sniped = false;

            // fetch the price
            var skinportPrice = item.SalePrice / 100f;
            var buffPrice = _buffClient.GetPrice(item.Category, item.MarketName, item.Float, item.Style);

            // calculate profits
            var profit = ((buffPrice * 97.5f) / 100f) - skinportPrice;
            var threshold = (skinportPrice * 15f) / 100f;

            // check threshold
            if (profit > threshold)
            {
                if (ConfigHandler.Get().Status)
                {
                    // add to basket & generate google captcha token
                    var watch2 = Stopwatch.StartNew();

                    var basket = _httpHandler.AddToBasket(item);
                    var token = _seleniumHandler.GenerateTokenAsync();
                    await Task.WhenAll(basket, token);

                    watch2.Stop();

                    if (basket.Result)
                    {
                        // order the item
                        var order = await _httpHandler.CreateOrder(item, token.Result);
                        watch.Stop();

                        // log the result
                        if (order != null && order.Success)
                        {
                            sniped = true;
                            Trace.WriteLine($"(Sniped): {watch.ElapsedMilliseconds}ms ({watch2.ElapsedMilliseconds}ms)");
                        }
                        else if (order != null)
                        {
                            Trace.WriteLine($"(Failed): {order.Message} = {watch.ElapsedMilliseconds}ms ({watch2.ElapsedMilliseconds}ms)");
                        }
                    }
                    else
                    {
                        Trace.WriteLine($"(Failed): {basket.Result} = {watch.ElapsedMilliseconds}ms ({watch2.ElapsedMilliseconds}ms)");
                    }
                }

                // send to telegram
                var telegramItem = new TelegramItem();
                telegramItem.ListingPrice = skinportPrice;
                telegramItem.BuffPrice = buffPrice;
                telegramItem.Profit = profit;
                telegramItem.Sniped = sniped;
                telegramItem.TimeTaken = watch.ElapsedMilliseconds;

                _telegramClient.SendItem(item, telegramItem);
            }
            else
            {
                watch.Stop();
            }

            // output data
            Trace.WriteLine($"{item.MarketName} ({item.Float})\n" +
                $"- Skinport: £{skinportPrice}\n" +
                $"- Buff: £{buffPrice}\n" +
                $"- Profit: £{profit.ToString("F")}\n" +
                $"- Threshold: £{threshold}\n" +
                $"- Elapsed: {watch.ElapsedMilliseconds}ms");
        }
    }
}

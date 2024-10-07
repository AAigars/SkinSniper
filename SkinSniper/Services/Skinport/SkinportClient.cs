using SkinSniper.Config;
using SkinSniper.Services.Buff;
using SkinSniper.Services.Skinport.Http;
using SkinSniper.Services.Skinport.Http.Entities;
using SkinSniper.Services.Skinport.Socket;
using SkinSniper.Services.Telegram;
using SkinSniper.Services.Telegram.Entities;
using System.Diagnostics;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace SkinSniper.Services.Skinport
{
    internal class SkinportClient
    {
        private readonly TelegramClient _telegramClient;
        private readonly BuffClient _buffClient;

        private readonly HttpHandler _httpHandler;
        private readonly SocketHandler _socketHandler;

        public SkinportClient(
            TelegramClient telegramClient,
            BuffClient buffClient,
            string baseUrl = "https://skinport.com", 
            string userAgent = "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/128.0.0.0 Safari/537.36")
        {
            _telegramClient = telegramClient;
            _buffClient = buffClient;
            
            // instantiate http handler
            var cookie = ConfigHandler.Get().Skinport.Cookie;
            _httpHandler = new HttpHandler(baseUrl, userAgent, cookie);

            // instantiate socket handler
            _socketHandler = new SocketHandler(baseUrl, userAgent, cookie);
            _socketHandler.ItemReceived += OnItemReceived;

            // assign test command handler
            _telegramClient.TestSkinport += OnTestSkinport;
        }

        private async void OnTestSkinport(object? sender, dynamic data)
        {
            // get telegram data from anonymous class
            ITelegramBotClient client = data.Client;
            Message message = data.Message;

            // execute test
            var item = new Item()
            {
                SaleId = 49226174,
                SalePrice = 211
            };
            
            var watch = Stopwatch.StartNew();
            
            var basket = _httpHandler.AddToBasket(item);
            var token = _httpHandler.GetTurnstileToken(item.SaleId);
            await Task.WhenAll(basket, token);
            
            var minTime = TimeSpan.FromMilliseconds(3000);
            if (watch.Elapsed < minTime) Thread.Sleep(minTime - watch.Elapsed);
            
            var order = await _httpHandler.CreateOrder(item, token.Result.token);
            await client.SendTextMessageAsync(message.Chat.Id, 
                $"Performance Results\n\n" +
                $" - Basket: {(basket.Result.Success ? '✅' : '❌')} {(basket.Result.Message != null ? $"({basket.Result.Message})" : "")}\n" +
                $" - Order: {(order.Success ? '✅' : '❌')} {(order.Message != null ? $"({order.Message})" : "")}\n" +
                $" - Captcha: {token.Result.token.Length}\n\n" +
                $"Time Taken: {watch.ElapsedMilliseconds}ms");
        }

        private async void OnItemReceived(object? sender, Item item)
        {
            // measure execution time
            var globalWatch = Stopwatch.StartNew();
            var sniped = false;

            // fetch the price
            var skinportPrice = item.SalePrice / 100;
            var buffPrice = _buffClient.GetPrice(item.Category, item.MarketName, item.Float, item.Style);

            // ignore items with no buff price or sticker value
            if (buffPrice == 0.0)
            {
                Trace.WriteLine($"No price data for {item.MarketName} ({item.Style})");
                return;
            }

            // calculate profits
            var profit = buffPrice - skinportPrice; //((buffPrice * 97.5m) / 100m) - skinportPrice;
            var threshold = skinportPrice * 0.15;

            // check threshold
            if (profit > threshold)
            {
                if (ConfigHandler.Get().Status)
                {
                    var watch = Stopwatch.StartNew();
            
                    var basket = _httpHandler.AddToBasket(item);
                    var token = _httpHandler.GetTurnstileToken(item.SaleId);
                    await Task.WhenAll(basket, token);
            
                    var minTime = TimeSpan.FromMilliseconds(3000);
                    if (watch.Elapsed < minTime) Thread.Sleep(minTime - watch.Elapsed);
            
                    var order = await _httpHandler.CreateOrder(item, token.Result.token);
                    if (basket.Result is { Success: true })
                    {
                        if (order != null && order.Success)
                        {
                            sniped = true;
                            Trace.WriteLine($"(Sniped): {watch.ElapsedMilliseconds}ms");
                        }
                        else if (order != null)
                        {
                            Trace.WriteLine($"(Failed): {order.Message} = {watch.ElapsedMilliseconds}ms");
                        }
                    }
                    else if (basket.Result != null)
                    {
                        Trace.WriteLine($"(Failed): {basket.Result.Message} = {watch.ElapsedMilliseconds}ms");
                    }
                }
            }

            // log any profit above £0
            if (profit > 0)
            {
                // send to telegram
                var telegramItem = new TelegramItem()
                {
                    ListingPrice = skinportPrice,
                    BuffPrice = buffPrice,
                    Profit = profit,
                    Sniped = sniped,
                    TimeTaken = globalWatch.ElapsedMilliseconds
                };

                _telegramClient.SendItem(item, telegramItem);
            }

            // output data
            var builder = new StringBuilder();
            builder.AppendLine($"{item.MarketName} ({item.Float})");

            if (item.Stickers != null && item.Stickers.Count > 0)
            {
                foreach (var sticker in item.Stickers)
                {
                    builder.AppendLine($"- {sticker.Name} - £{sticker.Value}");
                }

                builder.AppendLine();
            }

            builder.Append(
                $"- Skinport: £{skinportPrice}\n" +
                $"- Buff: £{buffPrice}\n" +
                $"- Profit: £{profit.ToString("F")}\n" +
                $"- Threshold: £{threshold}\n" +
                $"- Elapsed: {globalWatch.ElapsedMilliseconds}ms");

            Trace.WriteLine(builder.ToString());
        }
    }
}

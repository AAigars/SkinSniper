using SkinSniper.Config;
using SkinSniper.Services.Buff;
using SkinSniper.Services.Skinport.Http;
using SkinSniper.Services.Skinport.Http.Entities;
using SkinSniper.Services.Skinport.Puppeteer;
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
        private readonly string _baseUrl;
        private readonly string _userAgent;

        private HttpHandler? _httpHandler;
        private HttpHandler? _http2Handler;

        private SocketHandler? _socketHandler;
        private PuppeteerHandler _puppeteerHandler;

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


            //_puppeteerHandler = new PuppeteerHandler();
            //_puppeteerHandler.LoggedIn += OnLoggedIn;
            //_puppeteerHandler.Start();

            OnLoggedIn(this, "i18n=en; connect.sid=s%3AEvcM2uFLmShGdnIvE7GAlkRD1Qt_f3Nj.OL0pVMhpZqVrqJLrJqDRDu4iNuzs8xhyO3w7SHh1hCE; _csrf=iAVizMKwT_T5erbw_3Tl0umE; scid=eyJlbmMiOiJBMTI4Q0JDLUhTMjU2IiwiYWxnIjoiQTE5MktXIn0.bnMl5PCswSfICk8xohNWUuXQUgYsMYRzWU9rxomFrsJOcvG8Ttm3FQ.V8HgUOcDUq5WhFnxyTE4uw.09EoipCl2lTqqUF9NKFs9ps9OrvSKyNN5qdXx84U4gmui-xaMpUIptPR_pTI2tXfKQ-e9eFBPFBdm9RFpDPOuhrQHsc60Bg-q4OmHEkKNVtgLik1TCA_XEubw419LGb4FSMdTz8IZCsFYbiOf8xlVZjLcRKaPRSSDHEi9lMkoVZDC5Baem-wACqj-Hnj7_OqzNypE6o-BZ8wJ5-AqjAXfWAN5fY-2fPa2hr4lT6u2dSIP55J3JgmJsmykmMMWTX4noO7WMrXyrgdgaQlLS6i-E-lr_rzbQE-oJMlaj29jRFTSGCHudld_O49KAx_Yt5_dPFRwSto10k3Wepw22vgLReXyq5HIQzjsnTy7__SHv_gR172vh0dGP9dWc2mNELzA1-BCtvh5NGJDNxJq9KGSgobA_inx_D1_ivGR7YawpeGX5V87nx-an-EvAdiIJ7nC1Cs4r7u05n49EF5WuPbir47S0BgC2i9m6p76-LfrvuMh-seoan0zlNkpNO5SqQyIvPoWtgsnoAujMOmTY0T0Bz15shC-2sUcwIAHqq7NOiigcDeT771yj0kwk0NQoxvj00PEMnbNh8jspHvPrL_T0OLdH3oHg_isJ7JSlb3Ug1tljXn3usZfZEo1GDNERbURcPZKcqtCB45ZOlcwJkybLnScKf0RO_h2bL0SVYwp27tkK7_X3pNjBkyWqs2buaW.Es1yGHlbPwT6MYPrUUL8Ag; cf_clearance=oJUPPJR.IoVPT8vnDzvAxAt4vz9q90PgpqYiphEamlM-1728088572-1.2.1.1-joWBBd7WsTf6MJ_5_ugPF4GRfI5y7Gt8rC21dmmHFQJxz6Qdzbq50eXzAJLYS3pzk_6pTotz7wd7QLTiyH9rfMKs0KjijLtMewigq89afuO.QIyFJxsb9d4_1_3L3XgY0tYI_3D4Swnt0MyZZBOifrvqSYMp6uKv0QZCsDaCnVfi2ks2wTDWEGvd8xPca179gavColqj2omusJ4ZIxDKlhr4S2Qovrl4CCQOwiW1OWOoBqn.bRJyDZIE9e88a_nkpgONZS4KsD6oGff1YJBvSWg4xqRikKEpN8aBIZ_yqxCiHUNf6xN.idMvijm2DtAh71QpMvTEAzSCbQGDRWao9zKdiXOT.9gasJQig5ZVqMlF59KJShi4rF2eivppqRnU; __cf_bm=UXhrH5wM.OHMRwIL5G5rqEYzWDy2ctN_6ijLMkhEDOU-1728088572-1.0.1.1-HocMW4Yw68kuGbU5c6KAOOZQscRdCfZgnvsB0iN.EfvrQrVSF8dgEOphuDuuA7klheHGZfwAbKseZh7_Y.C0aw");
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
                SaleId = 49226139,
                SalePrice = 212
            };

            // measure execution time
            var watch = Stopwatch.StartNew();

            // add to basket & generate google captcha token
            //var basket = _httpsHandler.SendPostAsync(
            //    "/api/cart/add", $"sales[0][id]={item.SaleId}&sales[0][price]={item.SalePrice}&_csrf={_httpHandler._csrf}");
            var basket = _puppeteerHandler.AddToBasket(item);
            //var order = _puppeteerHandler.CreateOrder(item);

            await Task.WhenAll(basket);//, order);

            // output test data
            watch.Stop();
            await client.SendTextMessageAsync(message.Chat.Id, 
                $"Performance Results\n\n" +
                //$" - Basket: {(basket.Result.Success ? '✅' : '❌')} {(basket.Result.Message != null ? $"({basket.Result.Message})" : "")}\n" +
                //$" - Order: {(order.Result.Success ? '✅' : '❌')} {(order.Result.Message != null ? $"({order.Result.Message})" : "")}\n" +
                //$" - Captcha: {token.Length}\n\n" +
                $"Time Taken: {watch.ElapsedMilliseconds}ms");
        }

        private async void OnLoggedIn(object? sender, string cookie)
        {
            // instantiate http handler
            //_httpHandler = new HttpHandler(_baseUrl, _userAgent, cookie);
            //_http2Handler = new HttpHandler(_baseUrl, _userAgent, cookie);

            // instantiate socket handler
            _socketHandler = new SocketHandler(_baseUrl, _userAgent, cookie);
            _socketHandler.ItemReceived += OnItemReceived;

            // assign test command handler
            _telegramClient.TestSkinport += OnTestSkinport;
        }

        private async void OnItemReceived(object? sender, Item item)
        {
            // inspect stickers
            var stickerValue = 0.0m;

            if (item.Stickers != null && item.Stickers.Count > 0)
            {
                foreach (var sticker in item.Stickers)
                {
                    var value = _buffClient.GetPrice(null, $"Sticker | {sticker.Name}", null);
                    sticker.Value = value;
                    stickerValue += value;
                }
            }

            // measure execution time
            var watch = Stopwatch.StartNew();
            var sniped = false;

            // fetch the price
            var skinportPrice = item.SalePrice / 100m;
            var buffPrice = _buffClient.GetPrice(item.Category, item.MarketName, item.Float, item.Style);

            // ignore items with no buff price or sticker value
            if (buffPrice == 0.0m && stickerValue == 0.0m)
            {
                Trace.WriteLine($"No price data for {item.MarketName} ({item.Style})");
                return;
            }

            // calculate profits
            var profit = buffPrice - skinportPrice; //((buffPrice * 97.5m) / 100m) - skinportPrice;
            var threshold = skinportPrice * 0.15m;

            // check threshold
            if (profit > threshold)
            {
                if (ConfigHandler.Get().Status)
                {
                    // add to basket & generate recaptcha token
                    var basket = _httpHandler.AddToBasket(item);
                    var order = _http2Handler.CreateOrder(item, "");//_puppeteerHandler.GetToken());

                    await Task.WhenAll(basket, order);
                    watch.Stop();

                    if (basket.Result != null && basket.Result.Success)
                    {
                        // log the result
                        if (order != null && order.Result.Success)
                        {
                            sniped = true;
                            Trace.WriteLine($"(Sniped): {watch.ElapsedMilliseconds}ms");
                        }
                        else if (order != null)
                        {
                            Trace.WriteLine($"(Failed): {order.Result.Message} = {watch.ElapsedMilliseconds}ms");
                        }
                    }
                    else
                    {
                        Trace.WriteLine($"(Failed): {basket.Result.Message} = {watch.ElapsedMilliseconds}ms");
                    }
                }
            }
            else
            {
                watch.Stop();
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
                    TimeTaken = watch.ElapsedMilliseconds
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
                $"- Elapsed: {watch.ElapsedMilliseconds}ms");

            Trace.WriteLine(builder.ToString());
        }
    }
}

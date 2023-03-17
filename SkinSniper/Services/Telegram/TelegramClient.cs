using SkinSniper.Config;
using SkinSniper.Services.Skinport.Http.Entities;
using SkinSniper.Services.Telegram.Entities;
using System.Diagnostics;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;

namespace SkinSniper.Services.Telegram
{
    internal class TelegramClient
    {
        private TelegramBotClient _client;

        public TelegramClient()
        {
            // instantiate telegram
            _client = new TelegramBotClient(ConfigHandler.Get().Telegram.Token);

            // assign update events
            _client.StartReceiving(HandleUpdateAsync, HandlePollingErrorAsync);

            // grab basic data about bot
            var me = _client.GetMeAsync();
            me.Wait();
            Trace.WriteLine($"(Telegram): {me.Result.FirstName} ({me.Result.Id})");
        }

        public async void SendItem(Item item, TelegramItem tItem)
        {
            foreach (var user in ConfigHandler.Get().Telegram.Users)
            {
                await _client.SendPhotoAsync(user, $"https://community.cloudflare.steamstatic.com/economy/image/{item.Image}/512fx384f", 
                    $"{item.MarketName}\n" +
                    $"- 🔍 Float: {item.Float}\n" +
                    $"- ✏ Style: {item.Style}\n" +
                    $"- 💰 Price: £{tItem.ListingPrice.ToString("F")} (£{tItem.BuffPrice.ToString("F")})\n" +
                    $"- 📈 Profit: £{tItem.Profit.ToString("F")} ({((tItem.Profit / tItem.ListingPrice) * 100).ToString("F")}%)\n" +
                    $"\n" +
                    $"Status: {(tItem.Sniped ? "✅" : "❌")} ({tItem.TimeTaken}ms)\n" +
                    $"Link: https://skinport.com/item/{item.Url}/{item.SaleId}");
            }
        }
                                                                                                                                                                                                    
        private async Task HandleUpdateAsync(ITelegramBotClient client, Update update, CancellationToken token)
        {
            // ensure message
            if (update.Message is not { } message)
                return;

            // ensure text
            if (message.Text is not { } messageText)
                return;

            // get arguments
            var args = messageText.Split(' ');
            if (args.Length < 1)
                return;

            // parse arguments
            if (args[0] == "/join")
            {
                ConfigHandler.Get().Telegram.Users.Add(message.Chat.Id);
                ConfigHandler.Save();

                await client.SendTextMessageAsync(message.Chat.Id, "✅ You will now be notified of any potential snipes.");
            }
            else if (args[0] == "/start")
            {
                ConfigHandler.Get().Status = true;
                ConfigHandler.Save();

                await client.SendTextMessageAsync(message.Chat.Id, "✅ Skins will now be sniped automatically.");
            }
            else if (args[0] == "/stop")
            {
                ConfigHandler.Get().Status = false;
                ConfigHandler.Save();

                await client.SendTextMessageAsync(message.Chat.Id, "❌ Skins will no-longer be sniped automatically.");
            }
        }

        private Task HandlePollingErrorAsync(ITelegramBotClient client, Exception exception, CancellationToken token)
        {
            var error = exception switch
            {
                ApiRequestException apiRequestException
                    => $"(Telegram): {apiRequestException.ErrorCode} - {apiRequestException.Message}",
                _ => exception.ToString()
            };

            Trace.WriteLine(error);
            return Task.CompletedTask;
        }
    }
}

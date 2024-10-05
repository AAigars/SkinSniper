using SkinSniper.Config;
using SkinSniper.Database;
using SkinSniper.Database.Entities;
using SkinSniper.Services.Buff.Entities;
using System.Diagnostics;
using System.Net.Http.Json;


namespace SkinSniper.Services.Buff
{
    internal class BuffScraper
    {
        private readonly HttpClient _client;
        private readonly DatabaseClient _database;
        private Dictionary<string, float[][]> _margins = new()
        {
            { "Knife", new []
                {
                    new[] { 0.0f, 0.07f },
                    new[] { 0.07f, 0.15f },
                    new[] { 0.15f, 0.18f },
                    new[] { 0.18f, 0.24f },
                    new[] { 0.24f, 0.27f },
                    new[] { 0.27f, 0.38f },
                    new[] { 0.38f, 0.45f },
                    new[] { 0.45f, 0.63f },
                    new[] { 0.63f, 1.0f },
                }
            },
            { "Gloves", new []
                {
                    new[] { 0.0f, 0.07f },
                    new[] { 0.07f, 0.08f },
                    new[] { 0.08f, 0.09f },
                    new[] { 0.09f, 0.1f },
                    new[] { 0.1f, 0.13f },
                    new[] { 0.13f, 0.15f },
                    new[] { 0.15f, 0.18f },
                    new[] { 0.18f, 0.21f },
                    new[] { 0.21f, 0.24f },
                    new[] { 0.24f, 0.27f },
                    new[] { 0.27f, 0.38f },
                    new[] { 0.38f, 0.41f },
                    new[] { 0.41f, 0.45f },
                    new[] { 0.45f, 0.63f },
                    new[] { 0.63f, 1.0f },
                }
            }
        };
        private Dictionary<string, float[]> _ranges = new()
        {
            { "Factory New", new[] { 0.0f, 0.07f } },
            { "Minimal Wear", new[] { 0.07f, 0.15f } },
            { "Field-Tested", new[] { 0.15f, 0.38f } },
            { "Well-Worn", new[] { 0.38f, 0.45f } },
            { "Battle-Scarred", new[] { 0.45f, 1.0f } }
        };

        public BuffScraper(DatabaseClient database)
        {
            _database = database;

            var socketHandler = new SocketsHttpHandler()
            {
                UseCookies = false
            };

            // instantiate client
            _client = new HttpClient(socketHandler)
            {
                BaseAddress = new Uri($"https://buff.163.com/")
            };

            // set default headers
            //_client.DefaultRequestHeaders.Add("Cookie", ConfigHandler.Get().Buff.Cookie);
            _client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/110.0.0.0 Safari/537.36");
        }

        public async Task<BuffGoods?> GetGoods(string category, int page, bool group = true)
        {
            var request = await _client.GetAsync($"api/market/goods" +
                $"?game=csgo" +
                $"&page_num={page}" +
                $"&{(group ? "category_group" : "category")}={category}");

            try
            {
                var response = await request.Content.ReadFromJsonAsync<BuffGoods>();
                return response;
            } 
            catch
            {
                return null;
            }
        }

        public async void ScrapeGoods(string category, bool group = true)
        {
            var page = 0;
            var maxPage = -1;

            while (page++ != maxPage)
            {
                var goods = await GetGoods(category, page, group);
                if (goods != null && goods.Data != null && goods.Code == "OK")
                {
                    foreach (var item in goods.Data.Items)
                    {
                        Trace.WriteLine($"{item.Name} [{item.Id}]");
                        DatabaseItem.AddItem(_database, item.Id, item.Name);
                    }

                    maxPage = goods.Data.Total_Page;
                    Trace.WriteLine($"(Goods): {page}/{maxPage} = {goods.Code}");
                } 
                else
                {
                    Trace.WriteLine($"(Goods): {page}/{maxPage} = RETRY");
                    page--;
                }
            }
        }

        public async Task<BuffRecords?> GetRecords(DatabaseItemModel item)
        {
            try
            {
                using (var message = new HttpRequestMessage(HttpMethod.Get, $"api/market/goods/bill_order" +
                    $"?game=csgo" +
                    $"&goods_id={item.Id}"))
                {
                    message.Headers.Referrer = new Uri($"https://buff.163.com/goods/{item.Id}");

                    var request = await _client.SendAsync(message);
                    return await request.Content.ReadFromJsonAsync<BuffRecords>();
                }
            }
            catch
            {
                return null;
            }
        }

        public async Task<BuffListings?> GetListings(DatabaseItemModel item)
        {
            try
            {
                using (var message = new HttpRequestMessage(HttpMethod.Get, $"api/market/goods/sell_order" +
                    $"?game=csgo" +
                    $"&goods_id={item.Id}" +
                    $"&page_num=1" +
                    $"&sort_by=default"))
                {
                    message.Headers.Referrer = new Uri($"https://buff.163.com/goods/{item.Id}");

                    var request = await _client.SendAsync(message);
                    return await request.Content.ReadFromJsonAsync<BuffListings>();
                }
            }
            catch
            {
                return null;
            }
        }

        public async void Update()
        {
            var items = DatabaseItem.GetItems(_database);
                //.Where(item => item.UpdatedAt == 0);
                //.Where(item => DateTimeOffset.UtcNow.ToUnixTimeSeconds() - item.UpdatedAt > TimeSpan.FromHours(12).TotalSeconds);

            foreach (var (item, index) in items.Select((item, index) => (item, index)))
            {
                DatabaseItem.UpdateItem(_database, item);

                // fetch listings of item
                BuffListings? listings = null;

                while (listings == null || listings.Data == null || listings.Code != "OK")
                    listings = await GetListings(item);

                foreach (var listing in listings.Data.Items)
                {
                    Console.WriteLine($"{item.Name} = {listing.Price} [{listing.Id}]");
                    DatabaseListing.AddListing(_database, listing.Id, item.Id, decimal.Parse(listing.Price), double.Parse(listing.Asset_Info.PaintWear), listing.Asset_Info.Info.Phase_Data != null ? listing.Asset_Info.Info.Phase_Data.Name : "default", listing.Updated_At);
                }

                Trace.WriteLine($"(Listings): {index}/{items.Count()} = {listings.Data.Items.Count}");

                // fetch records of item
                /*BuffRecords? records = null;

                while (records == null || records.Data == null || records.Code != "OK")
                {
                    if (records != null) Console.WriteLine(records.Code);
                    records = await GetRecords(item);
                }

                foreach (var record in records.Data.Items)
                {
                    if (DatabaseRecord.Exists(_database, item.Id, record.Asset_Info.PaintWear != string.Empty ? double.Parse(record.Asset_Info.PaintWear) : null, record.Updated_At))
                        continue;

                    Console.WriteLine($"{item.Name} ({record.Asset_Info.PaintWear}) = {record.Price} [{record.Id}]");
                    DatabaseRecord.AddRecord(
                        _database, 
                        item.Id,
                        decimal.Parse(record.Price),
                        record.Asset_Info.PaintWear != string.Empty ? double.Parse(record.Asset_Info.PaintWear) : null, 
                        record.Asset_Info.Info.Phase_Data != null ? record.Asset_Info.Info.Phase_Data.Name : "default",
                        record.Updated_At);
                }

                Trace.WriteLine($"(Records): {index}/{items.Count()} = {records.Data.Items.Count}");*/
            }
        }
    }
}

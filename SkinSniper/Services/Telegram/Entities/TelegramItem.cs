using SkinSniper.Services.Skinport.Http.Entities;

namespace SkinSniper.Services.Telegram.Entities
{
    internal class TelegramItem
    {
        public decimal BuffPrice { get; set; }
        public decimal ListingPrice { get; set; }
        public decimal Profit { get; set; }
        public bool Sniped { get; set; }
        public long TimeTaken { get; set; }
    }
}

using SkinSniper.Services.Skinport.Http.Entities;

namespace SkinSniper.Services.Telegram.Entities
{
    internal class TelegramItem
    {
        public float BuffPrice { get; set; }
        public float ListingPrice { get; set; }
        public float Profit { get; set; }
        public bool Sniped { get; set; }
        public long TimeTaken { get; set; }
    }
}

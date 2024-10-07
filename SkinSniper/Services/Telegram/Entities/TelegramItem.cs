using SkinSniper.Services.Skinport.Http.Entities;

namespace SkinSniper.Services.Telegram.Entities
{
    internal class TelegramItem
    {
        public double BuffPrice { get; set; }
        public double ListingPrice { get; set; }
        public double Profit { get; set; }
        public bool Sniped { get; set; }
        public long TimeTaken { get; set; }
    }
}

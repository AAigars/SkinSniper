using SkinSniper.Services.Skinport.Http.Entities;
using System.Text.Json.Serialization;

namespace SkinSniper.Services.Skinport.Socket.Entities
{
    internal class SaleFeed
    {
        [JsonPropertyName("eventType")]
        public string? EventType { get; set; }

        [JsonPropertyName("sales")]
        public Item[] Sales { get; set; }
    }
}

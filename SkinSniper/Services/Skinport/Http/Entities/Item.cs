﻿using System.Text.Json.Serialization;

namespace SkinSniper.Services.Skinport.Http.Entities
{
    internal class Item
    {
        internal class Sticker
        {
            [JsonPropertyName("name")]
            public string Name { get; set; }
            public double? Value { get; set; }
        }

        [JsonPropertyName("saleId")]
        public int SaleId { get; set; }

        [JsonPropertyName("salePrice")]
        public int SalePrice { get; set; }
        
        [JsonPropertyName("saleStatus")]
        public string SaleStatus { get; set; }

        [JsonPropertyName("marketName")]
        public string MarketName { get; set; }

        [JsonPropertyName("category")]
        public string Category { get; set; }

        [JsonPropertyName("stattrak")]
        public bool StatTrack { get; set; }

        [JsonPropertyName("version")]
        public string Style { get; set; }

        [JsonPropertyName("wear")]
        public double? Float { get; set; }

        [JsonPropertyName("image")]
        public string Image { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("stickers")]
        public List<Sticker>? Stickers { get; set; }
    }
}

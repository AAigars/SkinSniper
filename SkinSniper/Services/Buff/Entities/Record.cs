using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkinSniper.Services.Buff.Entities
{
    internal class Record
    {
        public string? Id { get; set; }
        public string? Price { get; set; }
        public string? Float { get; set; }
        public string? Style { get; set; }

        [JsonProperty("updated_at")]
        public int UpdatedAt { get; set; }
    }
}

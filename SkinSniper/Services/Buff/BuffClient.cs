using Newtonsoft.Json;
using SkinSniper.Services.Buff.Entities;
using System.Diagnostics;

namespace SkinSniper.Services.Buff
{
    internal class BuffClient
    {
        private Dictionary<string, Item> _items = new();
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

        public BuffClient()
        {
            using (StreamReader reader = new StreamReader("buff.json"))
            {
                var items = JsonConvert.DeserializeObject<Dictionary<string, Item>>(reader.ReadToEnd());

                if (items != null)
                {
                    _items = items;
                    Trace.WriteLine($"(Buff): Loaded {_items.Count} items!");
                }
                else
                {
                    Trace.WriteLine("(Buff): Unable to load the buff data!");
                }
            }
        }

        public Item GetItem(string name)
        {
            return _items[name];
        }

        public float GetPrice(string category, string name, double? _float, string style)
        {
            // ignore items with no floats
            if (_float == null) return 0;

            // ignore vanilla knifes
            if (!name.Contains("|"))
                return 0;

            // get item from dataset
            var item = GetItem(name);
            if (item == null) return 0;

            // convert skinport to buff (version)
            if (style.Contains("Phase"))
                style = "P" + style.Last().ToString();

            // calculate margin
            var margin = CalculateMargin(category, _float);
            if (margin == null) return 0;

            // sort listings by updated, filter only listings within float margin and get only 10 recent
            var listings = item.Listings
                .Where(l => float.Parse(l.Float) > margin[0] && float.Parse(l.Float) < margin[1] && l.Style == style)
                .OrderBy(l => float.Parse(l.Price))
                .ThenBy(l => l.UpdatedAt);

            // check for listings
            var listingPrice = 0.0f;
            if (listings.Count() > 0)
            {
                // grab lowest listed price
                var lowestListing = listings.First();
                listingPrice = float.Parse(lowestListing.Price) * 0.12f;
            }

            // sort records by updated, filter only records within float margin and get only 10 recent
            var records = item.Records
                .Where(l => float.Parse(l.Float) > margin[0] && float.Parse(l.Float) < margin[1] && l.Style == style)
                .OrderBy(l => l.UpdatedAt)
                .Take(10);

            var recordPrice = 0.0f;
            if (records.Count() > 0)
            {
                // grab average record price
                recordPrice = records.Sum(r => float.Parse(r.Price) * 0.12f) / records.Count();
            }

            // select the lowest price from both
            if (listingPrice > 0.0f && recordPrice > 0.0f)
            {
                return MathF.Min(listingPrice, recordPrice);
            } 
            else
            {
                return listingPrice > 0.0f ? listingPrice : recordPrice;
            }
        }

        public float[]? CalculateMargin(string category, double? _float)
        {
            // get margins
            var margins = _margins[category];
            
            // iterate margins and find the upper and lower
            foreach (var margin in margins)
            {
                if (_float > margin[0] && _float < margin[1])
                {
                    return margin;
                }
            }

            // oh oh...
            return null;
        }
    }
}

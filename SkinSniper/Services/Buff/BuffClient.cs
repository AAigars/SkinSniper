using SkinSniper.Database;
using SkinSniper.Database.Entities;
using System.Diagnostics;

namespace SkinSniper.Services.Buff
{
    internal class BuffClient
    {
        private readonly DatabaseClient _database;

        private Dictionary<string, float[][]> _margins = new()
        {
            {
                "Knife", [
                    [0.0f, 0.07f],
                    [0.07f, 0.15f],
                    [0.15f, 0.18f],
                    [0.18f, 0.24f],
                    [0.24f, 0.27f],
                    [0.27f, 0.38f],
                    [0.38f, 0.45f],
                    [0.45f, 0.63f],
                    [0.63f, 1.0f]
                ]
            },
            {
                "Gloves", [
                    [0.0f, 0.07f],
                    [0.07f, 0.08f],
                    [0.08f, 0.09f],
                    [0.09f, 0.1f],
                    [0.1f, 0.13f],
                    [0.13f, 0.15f],
                    [0.15f, 0.18f],
                    [0.18f, 0.21f],
                    [0.21f, 0.24f],
                    [0.24f, 0.27f],
                    [0.27f, 0.38f],
                    [0.38f, 0.41f],
                    [0.41f, 0.45f],
                    [0.45f, 0.63f],
                    [0.63f, 1.0f]
                ]
            }
        };

        private Dictionary<Tuple<string, string>, double> _prices = new();

        public BuffClient(DatabaseClient database)
        {
            _database = database;

            // iterate over all items to calculate prices
            foreach (var item in DatabaseItem.GetItems(_database))
            {
                // find all possible styles
                var styles = DatabaseListing.GetListings(_database, item)
                    .GroupBy(listing => listing.Style)
                    .Select(group => group.First())
                    .ToList();

                // calculate price for each style
                foreach (var style in styles)
                {
                    // get listings and records
                    var listings = DatabaseListing.GetListings(_database, item)
                        .Where(l => l.Style == style.Style)
                        .OrderBy(l => l.Price);

                    var records = DatabaseRecord.GetRecords(_database, item)
                        .Where(l => l.Style == style.Style)
                        .OrderBy(l => l.Price);

                    // calculate price
                    var listingPrice = listings.Count() > 0 ? listings.First().Price * 0.11 : 0.0;
                    var recordPrice = records.Count() > 0 ? records.First().Price * 0.11 : 0.0;

                    // select lowest price
                    if (listingPrice > 0.0 && recordPrice > 0.0)
                    {
                        _prices.Add(new(item.Name, style.Style), Math.Min(listingPrice, recordPrice));
                    }
                    else
                    {
                        _prices.Add(new(item.Name, style.Style), listingPrice > 0.0 ? listingPrice : recordPrice);
                    }
                }
            }

            // woo we did it
            Trace.WriteLine($"(Buff): Processed {_prices.Count} prices!");
        }

        public double GetPrice(string category, string name, double? _float, string style = "default")
        {
            // get item from data set
            try
            {
                return _prices[new(name, style)];
            }
            catch
            {
                return 0.0;
            }

            /*
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
            */
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
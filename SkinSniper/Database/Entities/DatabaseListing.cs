using DbDataReaderMapper;
using Microsoft.Data.Sqlite;

namespace SkinSniper.Database.Entities
{
    public class DatabaseListingModel
    {
        [DbColumn("id")]
        public string Id { get; set; }

        [DbColumn("item_id")]
        public long ItemId { get; set; }

        [DbColumn("price")]
        public double Price { get; set; }

        [DbColumn("float")]
        public double Float { get; set; }

        [DbColumn("style")]
        public string Style { get; set; }

        [DbColumn("updated_at")]
        public long UpdatedAt { get; set; }
    }

    public static class DatabaseListing
    {
        private static List<DatabaseListingModel> _listings = new();

        public static DatabaseListingModel? AddListing(DatabaseClient database, string id, long itemId, decimal price, double _float, string style, long updatedAt)
        {
            // setup prepared statement
            var insert = new SqliteCommand("INSERT OR IGNORE INTO listings (id, item_id, price, float, style, updated_at) VALUES (?, ?, ?, ?, ?, ?) RETURNING *", database.GetConnection());
            insert.Parameters.Add(new SqliteParameter("id", id));
            insert.Parameters.Add(new SqliteParameter("item_id", itemId));
            insert.Parameters.Add(new SqliteParameter("price", price));
            insert.Parameters.Add(new SqliteParameter("float", _float));
            insert.Parameters.Add(new SqliteParameter("style", style));
            insert.Parameters.Add(new SqliteParameter("updated_at", updatedAt));

            // attempt to insert
            return database.ExecuteMappedQuery<DatabaseListingModel>(insert).FirstOrDefault();
        }

        public static List<DatabaseListingModel> GetListings(DatabaseClient database, DatabaseItemModel item)
        {
            if (_listings.Count == 0)
            {             
                // setup statement
                var query = new SqliteCommand("SELECT * FROM listings", database.GetConnection());

                // execute and return
                _listings.AddRange(database.ExecuteMappedQuery<DatabaseListingModel>(query));
            }

            return _listings.Where(listing => listing.ItemId == item.Id).ToList();
        }
    }
}

using DbDataReaderMapper;
using System.Data.SQLite;

namespace SkinSniper.Database.Entities
{
    public class DatabaseItemModel
    {
        [DbColumn("id")]
        public long Id { get; set; }

        [DbColumn("name")]
        public string Name { get; set; }

        [DbColumn("updated_at")]
        public long UpdatedAt { get; set; }

        public string GetWear()
        {
            var start = Name.IndexOf('(');
            var end = Name.IndexOf(')');

            return Name.Substring(start + 1, end - start - 1);
        }
    }

    public static class DatabaseItem
    {
        private static List<DatabaseItemModel> _items = new();

        public static void UpdateItem(DatabaseClient database, DatabaseItemModel item)
        {
            // update item
            var update = new SQLiteCommand("UPDATE items SET updated_at = ? WHERE id = ?", database.GetConnection());
            update.Parameters.Add(new SQLiteParameter("updated_at", DateTimeOffset.UtcNow.ToUnixTimeSeconds()));
            update.Parameters.Add(new SQLiteParameter("id", item.Id));

            // attempt to update
            database.ExecuteMappedQuery<DatabaseListingModel>(update);
        }

        public static DatabaseItemModel? AddItem(DatabaseClient database, int id, string name)
        {
            // setup prepared statement
            var query = new SQLiteCommand("INSERT OR IGNORE INTO items (id, name) VALUES (?, ?) RETURNING *", database.GetConnection());
            query.Parameters.Add(new SQLiteParameter("id", id));
            query.Parameters.Add(new SQLiteParameter("name", name));

            // attempt to insert
            return database.ExecuteMappedQuery<DatabaseItemModel>(query).FirstOrDefault();
        }

        public static DatabaseItemModel? GetItem(DatabaseClient database, string name)
        {
            if (_items.Count == 0)
            {
                // setup prepared statement
                var query = new SQLiteCommand("SELECT * FROM items", database.GetConnection());

                // execute and return
                _items.AddRange(database.ExecuteMappedQuery<DatabaseItemModel>(query));
            }

            return _items.Where(item => item.Name == name).FirstOrDefault();
        }

        public static List<DatabaseItemModel> GetItems(DatabaseClient database)
        {
            if (_items.Count == 0)
            {
                // setup prepared statement
                var query = new SQLiteCommand("SELECT * FROM items", database.GetConnection());

                // execute and return
                _items.AddRange(database.ExecuteMappedQuery<DatabaseItemModel>(query));
            }

            return _items.ToList();
        }
    }
}

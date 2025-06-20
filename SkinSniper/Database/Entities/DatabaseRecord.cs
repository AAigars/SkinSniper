﻿using DbDataReaderMapper;
using Microsoft.Data.Sqlite;

namespace SkinSniper.Database.Entities
{
    public class DatabaseRecordModel
    {
        [DbColumn("id")]
        public long Id { get; set; }

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

    public static class DatabaseRecord
    {
        private static List<DatabaseRecordModel> _records = new();

        public static DatabaseRecordModel? AddRecord(DatabaseClient database, long itemId, decimal price, double? _float, string style, long updatedAt)
        {
            // setup prepared statement
            var query = new SqliteCommand("INSERT OR IGNORE INTO records (item_id, price, float, style, updated_at) VALUES (?, ?, ?, ?, ?) RETURNING *", database.GetConnection());
            query.Parameters.Add(new SqliteParameter("item_id", itemId));
            query.Parameters.Add(new SqliteParameter("price", price));
            query.Parameters.Add(new SqliteParameter("float", _float));
            query.Parameters.Add(new SqliteParameter("style", style));
            query.Parameters.Add(new SqliteParameter("updated_at", updatedAt));

            // attempt to insert
            return database.ExecuteMappedQuery<DatabaseRecordModel>(query).FirstOrDefault();
        }

        public static List<DatabaseRecordModel> GetRecords(DatabaseClient database, DatabaseItemModel item)
        {
            if (_records.Count == 0)
            {
                // setup statement
                var query = new SqliteCommand("SELECT * FROM records", database.GetConnection());

                // execute and return
                _records.AddRange(database.ExecuteMappedQuery<DatabaseRecordModel>(query));
            }

            return _records.Where(record => record.ItemId == item.Id).ToList();
        }

        public static bool Exists(DatabaseClient database, long itemId, double? _float, long updatedAt)
        {
            if (_records.Count == 0)
            {
                // setup statement
                var query = new SqliteCommand("SELECT * FROM records", database.GetConnection());

                // execute and return
                _records.AddRange(database.ExecuteMappedQuery<DatabaseRecordModel>(query));
            }

            return _records.Any(record => record.ItemId == itemId && record.Float == _float && record.UpdatedAt == updatedAt);
        }
    }
}

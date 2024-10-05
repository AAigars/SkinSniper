using DbDataReaderMapper;
using System.Data.SQLite;

namespace SkinSniper.Database
{
    public class DatabaseClient
    {
        private SQLiteConnection _connection;

        /// <summary>
        /// Establishes a connection with the sql database
        /// </summary>
        public DatabaseClient()
        {
            // construct connection string
            var builder = new SQLiteConnectionStringBuilder()
            {
                DataSource = "data.db"
            };

            // instantiate connection
            _connection = new SQLiteConnection(builder.ConnectionString);

            // establish connection
            _connection.Open();
        }

        /// <summary>
        /// Retrieves the sql connection
        /// </summary>
        /// <returns>The sql connection</returns>
        public SQLiteConnection GetConnection()
        {
            return _connection;
        }

        /// <summary>
        /// Executes a query and maps it into an object
        /// </summary>
        /// <typeparam name="T">The class of the object</typeparam>
        /// <param name="command">The sql command</param>
        /// <returns></returns>
        public T[] ExecuteMappedQuery<T>(SQLiteCommand command) where T : class
        {
            using (command)
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    var list = new List<T>();

                    while (reader.Read())
                    {
                        list.Add(reader.MapToObject<T>());
                    }

                    return list.ToArray();
                }
            }
        }

        /// <summary>
        /// Executes a query and maps it into an object
        /// </summary>
        /// <typeparam name="T">The class of the object</typeparam>
        /// <param name="query">The sql query</param>
        /// <returns></returns>
        public T[] ExecuteMappedQuery<T>(string query) where T : class
        {
            return ExecuteMappedQuery<T>(new SQLiteCommand(query, _connection));
        }

        /// <summary>
        /// Executes a query and casts to the template type
        /// </summary>
        /// <typeparam name="T">The template type</typeparam>
        /// <param name="command">The sql command</param>
        /// <returns></returns>
        public T[] ExecuteQuery<T>(SQLiteCommand command)
        {
            using (command)
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    var list = new List<T>();

                    while (reader.Read())
                    {
                        list.Add((T)reader.GetValue(0));
                    }

                    return list.ToArray();
                }
            }
        }

        /// <summary>
        /// Executes a query and casts to the template type
        /// </summary>
        /// <typeparam name="T">The template type</typeparam>
        /// <param name="query">The sql query</param>
        /// <returns></returns>
        public T[] ExecuteQuery<T>(string query)
        {
            return ExecuteQuery<T>(new SQLiteCommand(query, _connection));
        }
    }
}

using DbDataReaderMapper;
using Microsoft.Data.Sqlite;
using SkinSniper.Database.Entities;

namespace SkinSniper.Database
{
    public class DatabaseClient
    {
        private SqliteConnection _connection;

        /// <summary>
        /// Establishes a connection with the sql database
        /// </summary>
        public DatabaseClient()
        {
            // construct connection string
            var builder = new SqliteConnectionStringBuilder()
            {
                DataSource = "data.db"
            };

            // instantiate connection
            _connection = new SqliteConnection(builder.ConnectionString);

            // establish connection
            _connection.Open();
        }

        /// <summary>
        /// Retrieves the sql connection
        /// </summary>
        /// <returns>The sql connection</returns>
        public SqliteConnection GetConnection()
        {
            return _connection;
        }

        /// <summary>
        /// Executes a query and maps it into an object
        /// </summary>
        /// <typeparam name="T">The class of the object</typeparam>
        /// <param name="command">The sql command</param>
        /// <returns></returns>
        public T[] ExecuteMappedQuery<T>(SqliteCommand command) where T : class
        {
            using (command)
            {
                using (SqliteDataReader reader = command.ExecuteReader())
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
            return ExecuteMappedQuery<T>(new SqliteCommand(query, _connection));
        }

        /// <summary>
        /// Executes a query and casts to the template type
        /// </summary>
        /// <typeparam name="T">The template type</typeparam>
        /// <param name="command">The sql command</param>
        /// <returns></returns>
        public T[] ExecuteQuery<T>(SqliteCommand command)
        {
            using (command)
            {
                using (SqliteDataReader reader = command.ExecuteReader())
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
            return ExecuteQuery<T>(new SqliteCommand(query, _connection));
        }
    }
}

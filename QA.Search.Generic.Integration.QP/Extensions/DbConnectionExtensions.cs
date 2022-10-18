using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace QA.Search.Generic.Integration.QP.Extensions
{
    internal static class DbConnectionExtensions
    {
        private struct ConnectionToken : IDisposable
        {
            private readonly IDbConnection _connection;

            public ConnectionToken(IDbConnection connection)
            {
                _connection = connection;
            }

            public void Dispose()
            {
                _connection.Close();
            }
        }

        /// <summary>
        /// If connection is already open before `using()` statement
        /// then it stays open after `using()` statement
        /// <para/>
        /// If connection is closed before `using()` statement
        /// then it will be opened inside `using()` and closed after `using()`
        /// </summary>
        /// <exception cref="InvalidOperationException" />
        public static async Task<IDisposable> EnsureOpenAsync(
            this DbConnection connection, CancellationToken token)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));

            switch (connection.State)
            {
                case ConnectionState.Open:
                    return null;
                case ConnectionState.Broken:
                    connection.Close();
                    break;
                case ConnectionState.Closed:
                    break;
                default:
                    throw new InvalidOperationException("Connection already in use");
            }

            await connection.OpenAsync(token).ConfigureAwait(false);

            return new ConnectionToken(connection);
        }

        /// <summary>
        /// Read JSON from SqlConection using SQL Server 2016 FOR JSON PATH query syntax.
        /// https://docs.microsoft.com/en-us/sql/relational-databases/json/format-query-results-as-json-with-for-json-sql-server
        /// </summary>
        public static async Task<JToken> LoadJsonAsync(this DbConnection connection,
            string sqlQuery, CancellationToken token,
            params (string name, object value)[] parameters)
        {
            using DbCommand dbCommand = connection.CreateCommand();
            dbCommand.CommandText = sqlQuery;

            foreach (var (name, value) in parameters)
            {
                DbParameter parameter = dbCommand.CreateParameter();
                parameter.ParameterName = name;
                parameter.Value = value;

                dbCommand.Parameters.Add(parameter);
            }

            using DbDataReader dbDataReader = await dbCommand.ExecuteReaderAsync(CommandBehavior.SequentialAccess, token);

            if (!dbDataReader.HasRows)
                return null;

            StringBuilder jsonResult = new StringBuilder();
            while (await dbDataReader.ReadAsync(token))
                jsonResult.Append(dbDataReader.GetValue(0).ToString());

            if (jsonResult.Length == 0)
                return null;

            using StringReader stringReader = new StringReader(jsonResult.ToString());
            using JsonTextReader jsonReader = new JsonTextReader(stringReader);
            return await JToken.LoadAsync(jsonReader, token);
        }
    }
}

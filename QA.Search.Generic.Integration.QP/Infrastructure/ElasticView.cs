using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Npgsql;
using QA.Search.Generic.DAL.Models;
using QA.Search.Generic.DAL.Services;
using QA.Search.Generic.DAL.Services.Configuration;
using QA.Search.Generic.Integration.Core.Models;
using QA.Search.Generic.Integration.Core.Models.DTO;
using QA.Search.Generic.Integration.QP.Exceptions;
using QA.Search.Generic.Integration.QP.Extensions;
using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace QA.Search.Generic.Integration.QP.Infrastructure
{
    /// <summary>
    /// Базовый класс для настройки индексации выбранного контента QP
    /// </summary>
    /// <typeparam name="TContext">EF Core DbContext базы QP</typeparam>
    public abstract class ElasticView<TContext>
        where TContext : GenericDataContext
    {
        private readonly string _loadSql;
        private readonly string _countSql;

        private readonly ILogger _logger;
        private readonly ContextConfiguration _contextConfiguration;

        protected readonly DbConnection Connection;

        protected readonly TContext Db;

        protected readonly GenericIndexSettings genericIndexSettings;

        /// <summary>
        /// Короткое имя целевого индекса Elastic
        /// </summary>
        public abstract string IndexName { get; }

        /// <summary>
        /// Name of view. Like nameof(MobileAppsView)
        /// </summary>
        public abstract string ViewName { get; }

        /// <summary>
        /// Описание индексации контентов документов из БД QP:
        /// выбор полей, JOIN таблиц, условия фильтрации. <see cref="QueryableExtensions"/>
        /// </summary>
        protected abstract IQueryable Query { get; }

        protected ElasticView(TContext context, ILogger logger, IOptions<ContextConfiguration> contextConfigurationOption, IOptions<GenericIndexSettings> genericIndexSettingsOption)
        {
            _logger = logger;
            _contextConfiguration = contextConfigurationOption.Value;

            Db = context;

            genericIndexSettings = genericIndexSettingsOption.Value;

            switch (_contextConfiguration.SqlServerType)
            {
                case SqlServerType.MSSQL:
                    Connection = new SqlConnection(_contextConfiguration.ConnectionString);
                    break;
                case SqlServerType.PostgreSQL:
                    Connection = new NpgsqlConnection(_contextConfiguration.ConnectionString);
                    break;
            }

            _loadSql = new LoadQueryVisitor(context, contextConfigurationOption).Visit(Query);
            _countSql = new CountQueryVisitor(context, contextConfigurationOption).Visit(Query);
        }

        protected string MakePageUrl(string relativePath)
        {
            if (Uri.TryCreate(new Uri(genericIndexSettings.RootUrl), relativePath, out Uri pageUrl))
                return pageUrl.ToString();

            throw new PageUrlCannotMakeException(relativePath);
        }

        protected string CombinePathToRoot(QPAbstractItem abstractItem)
        {
            abstractItem = Db.QPAbstractItems
                .Where(item => item.ContentItemID == abstractItem.ContentItemID)
                .Include(item => item.Parent)
                .Single();

            if (abstractItem.Parent == null || abstractItem.Name.Equals("start_page", StringComparison.OrdinalIgnoreCase))
                return string.Empty;

            return CombinePathToRoot(abstractItem.Parent) + "/" + abstractItem.Name;
        }

        /// <summary>
        /// Инициализация View перед началом каждой индексации
        /// </summary>
        /// <param name="fromDate">Минимальная дата изменения статей, подлежащих индексации</param>
        public virtual async Task InitAsync(DateTime fromDate, CancellationToken token)
        {
            await Task.Yield();
        }

        /// <summary>
        /// Подсчет примерного количества документов для индексации
        /// </summary>
        /// <param name="fromDate">Минимальная дата изменения статей, подлежащих индексации</param>
        public virtual async Task<int> CountAsync(DateTime fromDate, CancellationToken token)
        {
            _logger.LogDebug($"{nameof(_countSql)} = {_countSql}; {nameof(fromDate)} = {fromDate}");

            using DbCommand dbCommand = Connection.CreateCommand();
            dbCommand.CommandText = _countSql;

            DbParameter parameter = dbCommand.CreateParameter();
            parameter.ParameterName = nameof(fromDate);
            parameter.Value = fromDate;

            dbCommand.Parameters.Add(parameter);

            using (await Connection.EnsureOpenAsync(token))
            {
                object result = await dbCommand.ExecuteScalarAsync(token);

                return result != null ? Convert.ToInt32(result) : 0;
            }
        }

        /// <summary>
        /// Выгрузка из БД партии документов размера <see cref="BatchSize"/>,
        /// начиная с заданного Id <paramref name="fromId"/>.
        /// </summary>
        /// <param name="fromDate">Минимальная дата изменения статей, подлежащих индексации</param>
        public virtual async Task<JObject[]> LoadAsync(LoadParameters loadParameters, CancellationToken token)
        {
            _logger.LogDebug($"{nameof(_loadSql)} = {_loadSql}; {nameof(loadParameters.FromID)} = {loadParameters.FromID}; " +
                $"{nameof(loadParameters.FromDate)} = {loadParameters.FromDate}; " +
                $"{nameof(loadParameters.ViewParameters.BatchSize)} = {loadParameters.ViewParameters.BatchSize}");

            using (await Connection.EnsureOpenAsync(token))
            {
                JToken response = await Connection.LoadJsonAsync(_loadSql, token,
                    ("fromId", loadParameters.FromID),
                    ("fromDate", loadParameters.FromDate),
                    ("batchSize", loadParameters.ViewParameters.BatchSize));

                return response is JArray array
                    ? array.Cast<JObject>().ToArray()
                    : Array.Empty<JObject>();
            }
        }
    }
}
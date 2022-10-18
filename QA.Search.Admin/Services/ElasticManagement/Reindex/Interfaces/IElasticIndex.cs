using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QA.Search.Admin.Services.ElasticManagement.Reindex.Interfaces
{
    /// <summary>
    /// Данные об отдельно взятом индексе Elastic
    /// </summary>
    public interface IElasticIndex
    {
        /// <summary>
        /// Алиас, назначенный индексу
        /// </summary>
        string Alias { get; }
        string AliasWithPrefix { get; }

        bool HasAlias { get; }

        /// <summary>
        /// Имя индекса (включая дату создания и префиксы)
        /// </summary>
        string FullName { get; }

        /// <summary>
        /// Имя индекса без даты создания
        /// </summary>
        string UIName { get; }

        /// <summary>
        /// Дата создания индекса
        /// </summary>
        DateTime? CreationDate { get; }

        /// <summary>
        /// Индексом нельзя управлять из админки
        /// </summary>
        bool Readonly { get; }
    }
}

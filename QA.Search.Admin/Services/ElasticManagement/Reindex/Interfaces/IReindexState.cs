using System.Collections.Generic;

namespace QA.Search.Admin.Services.ElasticManagement.Reindex.Interfaces
{
    public interface IReindexState
    {
        /// <summary>
        /// Данные загружены и могут быть отображены
        /// </summary>
        bool Loaded { get; }
        /// <summary>
        /// Общая ошибка
        /// </summary>
        /// <remarks>
        /// Недоступен Elastic или проблемы с подключением к БД
        /// </remarks>
        bool CommonError { get; }

        IEnumerable<IIndexesContainer> GetAllContainers();
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QA.Search.Admin.Services.ElasticManagement.Reindex.TasksManagement
{
    public enum ReindexTaskOperationStatus
    {
        Ok,
        SourceIndexNotFound,
        DbError,
        /// <summary>
        /// Для указанного исходного индекса есть активная задача переиндексации
        /// Вторая задача для него же не может быть создана
        /// </summary>
        ThereIsActiveTaskForSourceIndex,
        TaskNotFound,
        IncorrectNewStatus,
        IncorrectIndexesState,
        /// <summary>
        /// Данные еще не загружены фоновым обреботчиком.
        /// Операция не может быть выполнена
        /// </summary>
        DataIsNotLoaded,
        
        /// <summary>
        /// Невозможно создать новую задачу по причине общего сбоя
        /// </summary>
        ServiceCanNotExecuteTask,
    }
}

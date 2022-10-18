using Microsoft.Extensions.Options;
using QA.Integration.JsonApiServices.Core;
using System;
using System.Linq;
using System.Diagnostics;
using QA.Integration.JsonApiServices.Core.RequestParams;
using System.Text;
using Microsoft.Extensions.Logging;

namespace QA.Search.Admin.Services.IndexingApi
{
    /// <summary>
    /// Базовый сервис интеграции с API служб индексации
    /// </summary>
    public abstract class IndexingApiServiceBase : JsonBasedServiceBase<IndexingApiServiceConfiguration>
    {
        protected virtual string IntergationErrorMessage { get; } = "Ошибка при взаимодействии со службой индексации";


        protected ILogger Logger { get; }

        public IndexingApiServiceBase(IndexingApiServiceConfigurationSource configurationSource, 
            ILogger<IndexingApiServiceBase> logger)
            : base(configurationSource)
        {
            Logger = logger;
            LogWriteAction = WriteTraceLog;
        }

        private void WriteTraceLog(object message)
        {
            Logger.Log(LogLevel.Information, message.ToString());
        }
    }
}

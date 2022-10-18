using Microsoft.Extensions.Options;
using QA.Integration.JsonApiServices.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QA.Search.Admin.Services.IndexingApi
{
    /// <summary>
    /// Источник конфигурации для сервиса, взаимодействующего с API
    /// </summary>
    /// <remarks>
    /// Нужен исключительно для того, чтобы обеспечяить совместимость с библиотекой QA.Integration.JsonApiServices
    /// </remarks>
    public class IndexingApiServiceConfigurationSource : IConfigurationSource<IndexingApiServiceConfiguration>
    {
        private IndexingApiServiceConfiguration Configuration { get; }

        public IndexingApiServiceConfigurationSource(IOptions<IndexingApiServiceConfiguration> configuration)
        {
            Configuration = configuration.Value;
        }

        public IndexingApiServiceConfiguration GetConfig() => Configuration;
    }
}

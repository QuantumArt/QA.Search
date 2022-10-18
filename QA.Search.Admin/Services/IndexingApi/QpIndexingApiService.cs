using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using QA.Search.Admin.Errors;
using Microsoft.Extensions.Logging;
using QA.Search.Admin.Models.QpIndexingPage;
using QA.Search.Generic.Integration.QP.Models;

namespace QA.Search.Admin.Services.IndexingApi
{
    /// <summary>
    /// Базовый сервис интеграции с API службы индексации QP
    /// </summary>
    public class QpIndexingApiService : IndexingApiServiceBase
    {
        protected override string IntergationErrorMessage { get; } = "Ошибка при взаимодействии со службой индексации QP";

        private Dictionary<TargetQP, string> ApiPaths { get; }


        public QpIndexingApiService(IndexingApiServiceConfigurationSource configurationSource,
            ILogger<IndexingApiServiceBase> logger)
            : base(configurationSource, logger)
        {
            ApiPaths = new Dictionary<TargetQP, string>
            {
                { TargetQP.IndexingQP, "qp/indexing"},
                { TargetQP.IndexingQPUpdate, "qp/indexing/update"},
                { TargetQP.IndexingMedia, "media/indexing"},
                { TargetQP.IndexingMediaUpdate, "media/indexing/update"}
            };
        }

        public async Task<QpIndexingResponse> GetIndexingData(TargetQP targetQP)
        {
            var request = CreateRequest()
                .SetMethod(HttpMethod.Get)
                .SetPath(ApiPaths[targetQP]);
            var apiResult = await ExecuteRequest<IndexingQpContext>(request);
            if (!apiResult.IsSucceeded || !apiResult.RequestCompleted || apiResult.Data == null)
            {
                throw new BusinessError(IntergationErrorMessage);
            }
            var res = new QpIndexingResponse(apiResult.Data);
            return res;
        }

        public async Task StartIndexing(TargetQP targetQP)
        {
            var request = CreateRequest()
                .SetMethod(HttpMethod.Post)
                .SetPath($"{ApiPaths[targetQP]}/start");
            var apiResult = await ExecuteRequest(request);
            if (!apiResult.IsSucceeded || !apiResult.RequestCompleted)
            {
                throw new BusinessError(IntergationErrorMessage);
            }
        }

        public async Task StopIndexing(TargetQP targetQP)
        {
            var request = CreateRequest()
                .SetMethod(HttpMethod.Post)
                .SetPath($"{ApiPaths[targetQP]}/stop");
            var apiResult = await ExecuteRequest(request);
            if (!apiResult.IsSucceeded || !apiResult.RequestCompleted)
            {
                throw new BusinessError(IntergationErrorMessage);
            }
        }
    }
}

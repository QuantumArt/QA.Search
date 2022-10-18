using Newtonsoft.Json.Linq;
using QA.Search.Generic.Integration.Core.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace QA.Search.Generic.Integration.Core.Processors
{
    /// <summary>
    /// Composite that passes documents through many <see cref="DocumentMiddleware{TMarker}"/>
    /// </summary>
    public class DocumentMiddleware<TMarker>
        where TMarker : IServiceMarker
    {
        private readonly DocumentProcessor<TMarker>[] _processors;

        public DocumentMiddleware(IEnumerable<DocumentProcessor<TMarker>> processors)
        {
            _processors = processors.ToArray();
        }

        public async Task<JObject[]> ProcessAsync(JObject[] documents, CancellationToken stoppingToken)
        {
            foreach (var processor in _processors)
            {
                documents = await processor.ProcessAsync(documents, stoppingToken);
            }

            return documents.ToArray();
        }

        public async Task<JObject> ProcessAsync(JObject document, CancellationToken stoppingToken)
        {
            var documents = await ProcessAsync(new[] { document }, stoppingToken);

            return documents.FirstOrDefault();
        }
    }
}

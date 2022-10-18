using Newtonsoft.Json.Linq;
using QA.Search.Generic.Integration.Core.Services;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace QA.Search.Generic.Integration.Core.Processors
{
    /// <summary>
    /// Interceptor for preprocessing documents before indexing in Elastic.
    /// </summary>
    public abstract class DocumentProcessor<TMarker>
        where TMarker : IServiceMarker
    {
        /// <summary>
        /// If processing is asyncronous, you should implement this batch method
        /// </summary>
        public virtual Task<JObject[]> ProcessAsync(JObject[] documents, CancellationToken stoppingToken)
        {
            return Task.FromResult(documents?.Select(Process).ToArray());
        }

        /// <summary>
        /// If processing is syncronous, you should implement this method that handles single document
        /// </summary>
        public virtual JObject Process(JObject document) => document;
    }
}
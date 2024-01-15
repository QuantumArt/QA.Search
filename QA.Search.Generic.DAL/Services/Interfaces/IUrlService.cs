using Newtonsoft.Json.Linq;
using System.Threading;
using System.Threading.Tasks;

#nullable enable

namespace QA.Search.Generic.DAL.Services.Interfaces
{
    public interface IUrlService<TContext> where TContext : GenericDataContext
    {
        Task<string?> GetUrlToPageByNameAsync(string abstractItemName, CancellationToken cancellationToken);

        Task<string?> GetUrlToPageByIdAsync(int? itemId, CancellationToken cancellationToken);

        string BuildUri(string baseUrl, string? query = null);

        JArray UrlToJArray(string urlField, string url);
    }
}

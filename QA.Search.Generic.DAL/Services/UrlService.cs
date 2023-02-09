using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using QA.Search.Generic.DAL.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

#nullable enable

namespace QA.Search.Generic.DAL.Services
{
    public class UrlService<TContext> : IUrlService<TContext> where TContext : GenericDataContext
    {
        private readonly GenericDataContext _context;

        private const int StartPageExtensionId = 547;

        public UrlService(TContext context)
        {
            _context = context;
        }

        public async Task<string> GetUrlToPageByNameAsync(string abstractItemName, CancellationToken cancellationToken)
        {
            int? itemId = await _context.QPAbstractItems
                .Where(x => x.Name == abstractItemName)
                .Select(x => x.ContentItemID)
                .SingleAsync(cancellationToken);

            return await GetUrlPageAsync(itemId, cancellationToken);
        }

        public async Task<string> GetUrlToPageByIdAsync(int? itemId, CancellationToken cancellationToken) => await GetUrlPageAsync(itemId, cancellationToken);

        private async Task<string> GetUrlPageAsync(int? itemId, CancellationToken cancellationToken)
        {
            Stack<string> urls = new();
            bool isBaseFound = false;

            do
            {
                (int? parentId, string urlPart, int? extensionId) = await GetUrlPartAsync(itemId, cancellationToken);

                if (extensionId == StartPageExtensionId)
                {
                    string? baseUrl = await GetBaseSitePath(itemId, cancellationToken);
                    if (string.IsNullOrWhiteSpace(baseUrl))
                    {
                        throw new InvalidOperationException("Can't find base url.");
                    }

                    urls.Push(baseUrl);
                    isBaseFound = true;
                    continue;
                }

                urls.Push(urlPart);
                itemId = parentId;
            }
            while (!isBaseFound);

            return string.Join("/", urls);
        }

        private async Task<(int? parentId, string urlPart, int? extensionId)> GetUrlPartAsync(int? contentItemId, CancellationToken cancellationToken)
        {
            Tuple<int?, string, int?> item = await _context.QPAbstractItems
                .Where(x => x.ContentItemID == contentItemId)
                .Select(x => new Tuple<int?, string, int?>(x.ParentID, x.Name, x.ExtensionID))
                .SingleAsync(cancellationToken);

            return item.ToValueTuple();
        }

        private async Task<string?> GetBaseSitePath(int? contentId, CancellationToken cancellationToken)
        {
            return await _context.StartPage
                .Where(x => x.ItemId == contentId && x.Visible == true && x.Archive == false)
                .Select(x => x.DefaultHost)
                .SingleAsync(cancellationToken);
        }

        public string BuildUri(string baseUrl, string? query = null)
        {
            UriBuilder uriBuilder = new(baseUrl);

            if (!string.IsNullOrWhiteSpace(query))
            {
                uriBuilder.Query = query;
            }

            return uriBuilder.ToString();
        }

        public JArray UrlToJArray(string urlField, string url)
        {
            if (string.IsNullOrWhiteSpace(urlField))
            {
                throw new ArgumentException($"'{nameof(urlField)}' cannot be null or whitespace.", nameof(urlField));
            }

            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentException($"'{nameof(url)}' cannot be null or whitespace.", nameof(url));
            }

            JObject urls = new()
            {
                [urlField] = url
            };

            return new JArray(urls);
        }
    }
}

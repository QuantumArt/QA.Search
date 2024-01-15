using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using QA.Search.Generic.DAL.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using QA.Search.Generic.DAL.Models;

#nullable enable

namespace QA.Search.Generic.DAL.Services
{
    public class UrlService<TContext> : IUrlService<TContext> where TContext : GenericDataContext
    {
        private readonly GenericDataContext _context;

        private const int START_PAGE_EXTENSION_ID = 547;
        private const string BASE_SITE_PATH = "SEARCH_DEFAULT_HOST";

        public UrlService(TContext context)
        {
            _context = context;
        }

        public async Task<string?> GetUrlToPageByNameAsync(string abstractItemName, CancellationToken cancellationToken)
        {
            int itemId = await _context.QPAbstractItems
                .Where(x => x.Name == abstractItemName)
                .Select(x => x.ContentItemID)
                .SingleOrDefaultAsync(cancellationToken);

            if (itemId.Equals(default))
            {
                return null;
            }

            return await GetUrlPageAsync(itemId, cancellationToken);
        }

        public async Task<string?> GetUrlToPageByIdAsync(int? itemId, CancellationToken cancellationToken) => await GetUrlPageAsync(itemId, cancellationToken);

        private async Task<string?> GetUrlPageAsync(int? itemId, CancellationToken cancellationToken)
        {
            Stack<string> urls = new();
            bool isBaseFound = false;

            do
            {
                PageInfo? pageInfo = await GetUrlPartAsync(itemId, cancellationToken);

                if (pageInfo is null)
                {
                    return null;
                }

                if (pageInfo.ExtensionId == START_PAGE_EXTENSION_ID)
                {
                    string? baseUrl = await GetBaseSitePath(cancellationToken);
                    if (string.IsNullOrWhiteSpace(baseUrl))
                    {
                        throw new InvalidOperationException("Can't find base url.");
                    }

                    urls.Push(baseUrl);
                    isBaseFound = true;
                    continue;
                }

                urls.Push(pageInfo.UrlPart);
                itemId = pageInfo.ParentId;
            }
            while (!isBaseFound);

            return string.Join("/", urls);
        }

        private async Task<PageInfo?> GetUrlPartAsync(int? contentItemId, CancellationToken cancellationToken)
        {
            return await _context.QPAbstractItems
                .Where(x => x.ContentItemID == contentItemId)
                .Select(x => new PageInfo(x.ParentID, x.Name, x.ExtensionID))
                .SingleOrDefaultAsync(cancellationToken);
        }

        private async Task<string?> GetBaseSitePath(CancellationToken cancellationToken)
        {
            return await _context.AppSettings
                .AsNoTracking()
                .Where(x => x.Key == BASE_SITE_PATH)
                .Select(x => x.Value)
                .SingleOrDefaultAsync(cancellationToken);
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

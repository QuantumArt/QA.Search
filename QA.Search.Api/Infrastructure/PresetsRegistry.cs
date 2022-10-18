using QA.Search.Api.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace QA.Search.Api.Infrastructure
{
    /// <summary>
    /// Кеш пресетов запроса поиска
    /// </summary>
    public static class PresetsRegistry
    {
        private static readonly SemaphoreSlim AsyncSearchLock = new SemaphoreSlim(1);
        private static readonly SemaphoreSlim AsyncSuggestLock = new SemaphoreSlim(1);

        private static SearchRequest SearchPreset;
        private static SuggestRequest SuggestPreset;

        public static async ValueTask<SearchRequest> GetSearchPresetAsync()
        {
            if (SearchPreset == null)
            {
                try
                {
                    await AsyncSearchLock.WaitAsync();
                    if (SearchPreset == null)
                    {
                        var path = Path.Combine(AppContext.BaseDirectory, "JsonPresets/search.json");
                        string json = await File.ReadAllTextAsync(path);
                        SearchPreset = JsonConvert.DeserializeObject<SearchRequest>(json);
                    }
                }
                finally
                {
                    AsyncSearchLock.Release();
                }
            }
            return SearchPreset;
        }

        public static async ValueTask<SuggestRequest> GetSuggestPresetAsync()
        {
            if (SuggestPreset == null)
            {
                try
                {
                    await AsyncSuggestLock.WaitAsync();
                    if (SuggestPreset == null)
                    {
                        var path = Path.Combine(AppContext.BaseDirectory, "JsonPresets/suggest.json");
                        string json = await File.ReadAllTextAsync(path);
                        SuggestPreset = JsonConvert.DeserializeObject<SuggestRequest>(json);
                    }
                }
                finally
                {
                    AsyncSuggestLock.Release();
                }
            }
            return SuggestPreset;
        }
    }
}

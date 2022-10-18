using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QA.Search.Api.Services
{
    public class IndexTranspiler
    {
        private readonly Settings _settings;

        public IndexTranspiler(IOptions<Settings> options)
        {
            _settings = options.Value;
        }

        /// <summary>
        /// Преобразует сокращенные имена индексов вида "{name}", которые публично доступны для пользователей API,
        /// в алиасы Elastic вида "search.{name}" для выполнения непосредственных запросов
        /// </summary>
        public string IndexesWildcard(params string[] shortIndexNames)
        {
            return String.Join(',', FullIndexesNames(shortIndexNames));
        }

        /// <summary>
        /// Певое доступное имя индекса, которое не является wildcard:
        /// GET {name}/_analyze принимает только полные имена индексов или алиасов
        /// </summary>
        public string IndexNameForAnalyze(IEnumerable<string> shortIndexNames)
        {
            return FullIndexesNames(shortIndexNames)
                .FirstOrDefault(name => !name.StartsWith('-') && !name.Contains('*'));
        }

        private IEnumerable<string> FullIndexesNames(IEnumerable<string> shortIndexNames)
        {
            return shortIndexNames
                .SelectMany(group => group
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(alias => alias.StartsWith('-')
                        ? '-' + _settings.AliasPrefix + alias.Substring(1)
                        : _settings.AliasPrefix + alias));
        }
    }
}

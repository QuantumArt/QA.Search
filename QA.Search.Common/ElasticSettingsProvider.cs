using Microsoft.Extensions.Options;
using QA.Search.Common.Interfaces;
using QA.Search.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QA.Search.Common
{
    public class ElasticSettingsProvider : IElasticSettingsProvider
    {
        private readonly string _projectName;

        private const string AliasPrefix = "search";
        private const string IndexPrefix = $"index.{AliasPrefix}";

        private const string AliasPrefixFormat = $"{AliasPrefix}.{{0}}.";
        private const string IndexPrefixFormat = $"{IndexPrefix}.{{0}}.";

        private const string AliasFormat = "{0}.{1}";
        private const string IndexFormat = "{0}.{1}";

        private const string AliasMaskFormat = "{0}.*";
        private const string IndexMaskFormat = "{0}.*";

        private const string TemplateMaskFormat = "{0}*";

        public ElasticSettingsProvider(IOptions<ElasticSettings> settings)
        {
            _projectName = settings.Value.ProjectName
                .ToLowerInvariant()
                .TrimStart('.')
                .TrimEnd('.')
                .Trim();
        }

        public string GetAlias(string viewName)
        {
            viewName = viewName.ToLowerInvariant()
                .TrimStart('.')
                .TrimEnd('.')
                .Trim();

            return string.Format(AliasFormat, GetAliasPrefix().TrimEnd('.'), viewName);
        }

        public string GetIndexName(string viewName)
        {
            viewName = viewName.ToLowerInvariant()
               .TrimStart('.')
               .TrimEnd('.')
               .Trim();

            return string.Format(IndexFormat, GetIndexPrefix().TrimEnd('.'), viewName);
        }

        public string GetAliasMask() => string.Format(AliasMaskFormat, GetAliasPrefix().TrimEnd('.'));

        public string GetTemplateMask() => string.Format(TemplateMaskFormat, GetAliasPrefix().TrimEnd('.'));

        public string GetIndexMask() => string.Format(IndexMaskFormat, GetIndexPrefix().TrimEnd('.'));

        public string GetAliasPrefix() => string.Format(AliasPrefixFormat, _projectName);

        public string GetIndexPrefix() => string.Format(IndexPrefixFormat, _projectName);

        private IEnumerable<string> GetFullIndexesNames(IEnumerable<string> shortIndexNames)
        {
            return shortIndexNames
                .SelectMany(group => group
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(alias => alias.StartsWith('-')
                        ? $"-{GetAliasPrefix()}{alias[1..]}"
                        : $"{GetAliasPrefix()}{alias}"));
        }

        /// <summary>
        /// Преобразует сокращенные имена индексов вида "{name}", которые публично доступны для пользователей API,
        /// в алиасы Elastic вида "search.{name}" для выполнения непосредственных запросов
        /// </summary>
        public string GetIndexesWildcard(params string[] shortIndexNames)
        {
            return string.Join(',', GetFullIndexesNames(shortIndexNames));
        }

        /// <summary>
        /// Певое доступное имя индекса, которое не является wildcard:
        /// GET {name}/_analyze принимает только полные имена индексов или алиасов
        /// </summary>
        public string GetIndexNameForAnalyze(IEnumerable<string> shortIndexNames)
        {
            return GetFullIndexesNames(shortIndexNames)
                .FirstOrDefault(name => !name.StartsWith('-') && !name.Contains('*'));
        }
    }
}

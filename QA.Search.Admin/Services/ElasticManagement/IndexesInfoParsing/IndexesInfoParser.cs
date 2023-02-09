using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using QA.Search.Admin.Services.ElasticManagement.Reindex.Interfaces;
using QA.Search.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace QA.Search.Admin.Services.ElasticManagement.IndexesInfoParsing
{
    public class IndexesInfoParser
    {
        public class IndexInfo
        {
            public string FullName { get; set; }

            public string UIName { get; set; }

            public DateTime? CreationDate { get; set; }

            public List<string> Aliases { get; set; }
            public List<string> AliasesWithPrefixes { get; set; }

            public bool Readonly { get; set; }
        }

        string IndexPrefix { get; }
        string AliasPrefix { get; }
        string IndexParseRegexTemplate { get; }
        string DateTimeParsingFormat { get; }
        string[] ReadonlyPrefixes { get; }

        Regex IndexNameParseRegex { get; }

        public IndexesInfoParser(IElasticSettingsProvider elasticSettingsProvider, IOptions<IndexesInfoParserSettings> indexesInfoParserSettings, IOptions<Settings> commonSettings)
        {
            IndexParseRegexTemplate = indexesInfoParserSettings.Value.IndexParseRegexTemplate;
            DateTimeParsingFormat = indexesInfoParserSettings.Value.DateTimeParsingFormat;
            IndexPrefix = elasticSettingsProvider.GetIndexPrefix();
            AliasPrefix = elasticSettingsProvider.GetAliasPrefix();
            ReadonlyPrefixes = commonSettings.Value.ReadonlyPrefixes ?? Array.Empty<string>();
            IndexNameParseRegex = CreateIndexNameParseRegex();
        }

        private Regex CreateIndexNameParseRegex()
        {
            var indexParseRegex = IndexParseRegexTemplate
                .Replace("{IndexPrefix}", IndexPrefix.Replace(".", @"\."));
            return new Regex(indexParseRegex, RegexOptions.Compiled);
        }

        public List<IndexInfo> ParseIndexes(string response)
        {
            IDictionary<string, JToken> indexesDict = JObject.Parse(response);
            return indexesDict.Select(kvp => ParseSingleIndexData(kvp)).ToList();
        }

        private IndexInfo ParseSingleIndexData(KeyValuePair<string, JToken> kvp)
        {
            var fullIndexName = kvp.Key;
            var parsedData = ParseIndexFullName(fullIndexName);
            var uiName = parsedData.Item1;
            DateTime? created = parsedData.Item2;

            var aliasesWithPrefixes = kvp.Value
                .Children<JProperty>()
                .FirstOrDefault(jp => jp.Name == "aliases")?
                .Value
                .Children<JProperty>()
                .Select(jp => jp.Name);

            var aliases = aliasesWithPrefixes
                .Select(ProcessAliasName)
                .ToList();

            return new IndexInfo
            {
                Aliases = aliases,
                AliasesWithPrefixes = aliasesWithPrefixes.ToList(),
                CreationDate = created,
                FullName = fullIndexName,
                UIName = uiName,
                Readonly = GetIsReadonly(uiName)
            };


        }

        private bool GetIsReadonly(string uiName)
        {
            return ReadonlyPrefixes.Any(p => uiName.StartsWith(p));
        }

        private string ProcessAliasName(string aliasFullName)
        {
            if (aliasFullName.StartsWith(AliasPrefix))
            {
                aliasFullName = aliasFullName[AliasPrefix.Length..];
            }
            return aliasFullName;
        }

        public IndexInfo CreateDestinationIndexLocally(IElasticIndex sourceIndex, DateTime creationDateTime)
        {
            return new IndexInfo
            {
                UIName = sourceIndex.UIName,
                Aliases = new List<string>(),
                AliasesWithPrefixes = new List<string>(),
                CreationDate = creationDateTime,
                FullName = $"{IndexPrefix}{sourceIndex.UIName}.{creationDateTime.ToString(DateTimeParsingFormat).ToLower()}"
            };
        }

        internal IndexInfo CreateNewIndexInfo(string indexName)
        {
            var creationDateTime = DateTime.Now;
            indexName = indexName.ToLower();
            return new IndexInfo
            {
                UIName = indexName,
                Aliases = new List<string> { indexName },
                AliasesWithPrefixes = new List<string> { $"{AliasPrefix}{indexName}" },
                CreationDate = creationDateTime,
                FullName = $"{IndexPrefix}{indexName}.{creationDateTime.ToString(DateTimeParsingFormat).ToLower()}"
            };
        }

        private Tuple<string, DateTime?> ParseIndexFullName(string fullName)
        {
            var fullIndexName = fullName;
            var uiName = "";
            DateTime? created = null;

            var match = IndexNameParseRegex.Match(fullIndexName);
            if (match.Success)
            {
                uiName = match.Groups?.Cast<Group>().Skip(1).FirstOrDefault()?.Value ?? "";
                var dateStr = match.Groups?.Cast<Group>().Skip(2).FirstOrDefault()?.Value ?? "";
                if (DateTime.TryParseExact(
                    dateStr.ToUpper(),
                    DateTimeParsingFormat,
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out DateTime dateTime))
                {
                    created = dateTime;
                }
            }

            return new Tuple<string, DateTime?>(uiName, created);
        }

        internal IndexInfo CreateIndexInfoFromName(string fullName)
        {
            var parsedData = ParseIndexFullName(fullName);

            return new IndexInfo
            {
                Aliases = new List<string>(),
                AliasesWithPrefixes = new List<string>(),
                CreationDate = parsedData.Item2,
                FullName = fullName,
                UIName = parsedData.Item1,
                Readonly = GetIsReadonly(parsedData.Item1)
            };
        }
    }
}

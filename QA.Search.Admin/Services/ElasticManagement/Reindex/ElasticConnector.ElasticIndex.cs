using QA.Search.Admin.Services.ElasticManagement.IndexesInfoParsing;
using QA.Search.Admin.Services.ElasticManagement.Reindex.Interfaces;
using System;
using System.Linq;

namespace QA.Search.Admin.Services.ElasticManagement.Reindex
{
    public partial class ElasticConnector
    {
        [System.Diagnostics.DebuggerDisplay("{Alias}")]
        private class ElasticIndex : IElasticIndex
        {
            public ElasticIndex(IndexesInfoParser.IndexInfo info)
            {
                Alias = info.Aliases?.FirstOrDefault() ?? string.Empty;
                AliasWithPrefix = info.AliasesWithPrefixes.FirstOrDefault() ?? string.Empty;
                FullName = info.FullName;
                UIName = info.UIName;
                CreationDate = info.CreationDate;
                Readonly = info.Readonly;
            }

            private ElasticIndex()
            { }

            public string Alias { get; set; }
            public string AliasWithPrefix { get; set; }
            public bool HasAlias { get => !string.IsNullOrWhiteSpace(Alias); }

            public string FullName { get; set; }

            public string UIName { get; set; }

            public DateTime? CreationDate { get; set; }

            public bool Readonly { get; set; }
        }
    }
}

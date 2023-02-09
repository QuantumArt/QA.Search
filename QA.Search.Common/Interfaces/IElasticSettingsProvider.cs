using System.Collections.Generic;

namespace QA.Search.Common.Interfaces
{
    public interface IElasticSettingsProvider
    {
        string GetAlias(string viewName);
        string GetIndexName(string viewName);
        string GetAliasMask();
        string GetTemplateMask();
        string GetIndexMask();
        public string GetAliasPrefix();
        public string GetIndexPrefix();
        string GetIndexesWildcard(params string[] shortIndexNames);
        string GetIndexNameForAnalyze(IEnumerable<string> shortIndexNames);
    }
}

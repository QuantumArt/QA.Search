using Newtonsoft.Json.Linq;
using QA.Search.Api.Models;

namespace QA.Search.Api.Services
{
    public class SnippetsTranspiler
    {
        /// <summary>
        /// Преобразует описание сниппетов в Query DSL "highlight"
        /// по общему полю "_shingles" или по указанным полям "*.shingles"
        /// </summary>
        public JObject BuildSearchSnippets(SnippetsExpression snippets) => BuildSnippets(snippets, "shingles");

        /// <summary>
        /// Преобразует описание сниппетов в Query DSL "highlight"
        /// по общему полю "_prefixes" или по указанным полям "*.prefixes"
        /// </summary>
        public JObject BuildSuggestSnippets(SnippetsExpression snippets) => BuildSnippets(snippets, "prefixes");

        private JObject BuildSnippets(SnippetsExpression snippets, string fieldSuffix)
        {
            var highlight = new JObject
            {
                ["order"] = "score",
                ["pre_tags"] = new JArray("<b>"),
                ["post_tags"] = new JArray("</b>"),
                ["require_field_match"] = false
            };

            var fields = new JObject();

            if (snippets.HasManySnippets)
            {
                foreach (var (field, fieldSnippet) in snippets)
                {
                    var fieldHighlight = new JObject();

                    if (fieldSnippet.Count != null)
                    {
                        fieldHighlight["number_of_fragments"] = fieldSnippet.Count;
                    }
                    if (fieldSnippet.Length != null)
                    {
                        fieldHighlight["fragment_size"] = fieldSnippet.Length;
                        fieldHighlight["no_match_size"] = fieldSnippet.Length;
                    }

                    fields[$"{field}.{fieldSuffix}"] = fieldHighlight;
                }
            }
            else
            {
                SnippetExpression allSnippet = snippets;

                if (allSnippet.Count != null)
                {
                    highlight["number_of_fragments"] = allSnippet.Count;
                }
                if (allSnippet.Length != null)
                {
                    highlight["fragment_size"] = allSnippet.Length;
                    highlight["no_match_size"] = allSnippet.Length;
                }

                fields[$"_{fieldSuffix}"] = new JObject();
            }

            highlight["fields"] = fields;

            return highlight;
        }
    }
}
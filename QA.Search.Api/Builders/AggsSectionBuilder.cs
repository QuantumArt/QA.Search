using QA.Search.Api.Models.ElasticSearch.RequestSections;

namespace QA.Search.Api.Builders
{
    public class AggsSectionBuilder
    {
        private string aggrName;
        private TermsSection terms;

        /// <summary>
        /// Установна названия агрегированного поля
        /// </summary>
        /// <param name="name"></param>
        public void SetAggrName(string name)
        {
            aggrName = name;
        }

        public void SetTermsField(string name)
        {
            terms = terms ?? new TermsSection();
            terms.Field = name;
        }

        public void SetTermsSize(int size)
        {
            terms = terms ?? new TermsSection();
            terms.Size = size;
        }

        public AggsSection Build()
        {
            var result = new AggsSection();
            if (!string.IsNullOrEmpty(aggrName))
            {
                var aggrValue = new AggsSectionValue
                {
                    Terms = terms
                };

                result.Add(aggrName, aggrValue);
            }
            return result;
        }
    }
}

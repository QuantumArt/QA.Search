using Microsoft.AspNetCore.Mvc;

namespace QA.Search.Api.Models
{
    /// <summary>
    /// Documentation-only Union of <see cref="SearchResponse"/> and <see cref="ProblemDetails" />
    /// </summary>
    public class MultiSearchResponse : SearchResponse
    {
        /// <summary>
        /// A URI reference[RFC3986] that identifies the problem type.T
        /// encourages that, when dereferenced, it provide human-readab
        /// the problem type (e.g., using HTML [W3C.REC-html5-20141028]
        /// is not present, its value is assumed to be "about:blank".
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// A human-readable explanation specific to this occurrence of the problem.
        /// </summary>
        public string Detail { get; set; }

        private MultiSearchResponse()
        {
        }
    }

    /// <summary>
    /// Documentation-only Union of <see cref="SuggestResponse"/> and <see cref="ProblemDetails" />
    /// </summary>
    public class MultiSuggestResponse : SuggestResponse
    {
        /// <summary>
        /// A URI reference[RFC3986] that identifies the problem type.T
        /// encourages that, when dereferenced, it provide human-readab
        /// the problem type (e.g., using HTML [W3C.REC-html5-20141028]
        /// is not present, its value is assumed to be "about:blank".
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// A human-readable explanation specific to this occurrence of the problem.
        /// </summary>
        public string Detail { get; set; }

        private MultiSuggestResponse()
        {
        }
    }

    /// <summary>
    /// Documentation-only Union of <see cref="CompletionResponse"/> and <see cref="ProblemDetails" />
    /// </summary>
    public class MultiCompletionResponse : CompletionResponse
    {
        /// <summary>
        /// A URI reference[RFC3986] that identifies the problem type.T
        /// encourages that, when dereferenced, it provide human-readab
        /// the problem type (e.g., using HTML [W3C.REC-html5-20141028]
        /// is not present, its value is assumed to be "about:blank".
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// A human-readable explanation specific to this occurrence of the problem.
        /// </summary>
        public string Detail { get; set; }

        private MultiCompletionResponse()
        {
        }
    }
}

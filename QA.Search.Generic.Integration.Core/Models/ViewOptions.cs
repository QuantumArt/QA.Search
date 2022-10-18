using System.Collections.Generic;

namespace QA.Search.Generic.Integration.Core.Models
{
    public class ViewOptions
    {
        /// <summary>
        /// Fallback for situation where in config not set BatchSize for view
        /// </summary>
        public int DefaultBatchSize { get; set; }

        /// <summary>
        /// Dictionary for linked "view->view Parameter". 
        /// View parameter class for storing some parameter/option for current view
        /// </summary>
        public Dictionary<string, ViewParameters> ViewParameters { get; set; }
    }
}

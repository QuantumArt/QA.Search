namespace QA.Search.Generic.Integration.Core.Models
{
    /// <summary>
    /// Class for storing some parameters/options for view. For future expanding
    /// </summary>
    public class ViewParameters
    {
        /// <summary>
        /// Size of pack for take from DB and send to Elastic
        /// </summary>
        public int BatchSize { get; set; }

        /// <summary>
        /// ALlow to disable this index view
        /// </summary>
        public bool IsDisabled { get; set; }
    }
}

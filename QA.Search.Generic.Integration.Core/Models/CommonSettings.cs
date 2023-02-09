namespace QA.Search.Generic.Integration.Core.Models
{
    public class CommonSettings
    {
        /// <summary>
        /// Run as Windows Service or as Console App
        /// </summary>
        public bool IsService { get; set; }

        /// <summary>
        /// Indicates index permissions or not
        /// </summary>
        public bool IndexPermissions { get; set; }

        /// <summary>
        /// Array of indexer library names (without extension)
        /// </summary>
        public string[] IndexerLibraries { get; set; }
    }
}
namespace QA.Search.Generic.DAL.Services.Configuration
{
    public enum ContentAccess
    {
        /// <summary>
        /// Published articles
        /// </summary>
        Live = 0,
        /// <summary>
        /// Splitted versions of articles
        /// </summary>
        Stage = 1,
        /// <summary>
        /// Splitted versions of articles including invisible and archived (overrides useDefaultFiltration content setting)
        /// </summary>
        StageNoDefaultFiltration = 2
    }
}

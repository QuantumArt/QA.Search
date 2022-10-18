namespace QA.Search.Common.Extensions
{
    public static class StringExtensions
    {
        public static string RemoveTrailingSlash(this string url)
        {
            return url != null && url.EndsWith('/') ? url.Substring(0, url.Length - 1) : url;
        }
    }
}
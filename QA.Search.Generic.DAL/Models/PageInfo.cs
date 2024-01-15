namespace QA.Search.Generic.DAL.Models;

public class PageInfo
{
    public int? ParentId { get; }
    public string UrlPart { get; }
    public int? ExtensionId { get; }

    public PageInfo(int? parentId, string urlPart, int? extensionId)
    {
        ParentId = parentId;
        UrlPart = urlPart;
        ExtensionId = extensionId;
    }
}

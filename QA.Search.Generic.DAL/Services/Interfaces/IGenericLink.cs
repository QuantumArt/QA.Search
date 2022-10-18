namespace QA.Search.Generic.DAL.Services.Interfaces
{
    public interface IGenericLink
    {
        int ID { get; set; }
        int LinkedItemID { get; set; }
        int LinkID { get; }

        IGenericItem Item { get; }
        IGenericItem LinkedItem { get; }
    }
}

namespace QA.Search.Admin.Services.ElasticManagement.Reindex.Interfaces
{
    public interface IReindexStateStore
    {
        IReindexState GetState();
    }
}

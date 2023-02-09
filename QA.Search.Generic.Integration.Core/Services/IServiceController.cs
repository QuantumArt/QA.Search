namespace QA.Search.Generic.Integration.Core.Services
{
    public interface IServiceController<TMarker>
        where TMarker : IServiceMarker
    {
        bool Start();
        bool Stop();
    }
}

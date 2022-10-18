namespace QA.Search.Generic.Integration.Core.Models
{
    /// <summary>
    /// Состояние одного сервиса индексации ScheduledService
    /// </summary>
    public enum IndexingState
    {
        Running = 0,
        Stopped = 1,
        AwaitingRun = 2,
        AwaitingStop = 3,
        Error = 4
    }
}

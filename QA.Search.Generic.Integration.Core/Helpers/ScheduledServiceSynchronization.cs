using System.Threading;

namespace QA.Search.Generic.Integration.Core.Helpers;

public class ScheduledServiceSynchronization
{
    public SemaphoreSlim Semaphore { get; } = new(1, 1);

    public int SemaphoreAwaitTime { get; } = 100;

    public string SemaphoreBusyMessage { get; } = "Выполняется другая задача. Выполнение текущей задачи пропущено. Задача будет запущена повторно согласно своему расписанию.";
}

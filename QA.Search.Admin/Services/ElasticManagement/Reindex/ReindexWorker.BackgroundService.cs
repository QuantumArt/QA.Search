using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace QA.Search.Admin.Services.ElasticManagement.Reindex
{
    public partial class ReindexWorker : BackgroundService, IDisposable
    {
        /// <summary>
        /// Токен, который свзяан с токеном, переданным в StartAsync
        /// </summary>
        private CancellationTokenSource LinkedTokenSource { get; set; }

        #region BackgroundService

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            LinkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
            var linkedToken = LinkedTokenSource.Token;
            bool taskCancelled = false;
            while (!taskCancelled)
            {
                try
                {
                    while (true)
                    {
                        await DoWork(linkedToken);
                        await Task.Delay(Settings.Interval, linkedToken);
                    }
                }
                catch (OperationCanceledException)
                {
                    taskCancelled = true;
                }
                catch (Exception commonEx)
                {
                    // По идее, сюда не придем никогда. Такие исключения должны быть обработаны в DoWork
                    Logger.LogError(commonEx, "Необработанное исключение при выполнении фоновой задачи в {0}", GetType().AssemblyQualifiedName);
                    await Task.Delay(Settings.Interval, linkedToken);
                }
            }
            LinkedTokenSource = null;
        }

        #endregion

        #region  IDisposable

        private bool IsDisposed { get; set; } = false;

        public override void Dispose()
        {
            base.Dispose();
            IsDisposed = true;
            LinkedTokenSource?.Cancel();
        }

        #endregion
    }
}
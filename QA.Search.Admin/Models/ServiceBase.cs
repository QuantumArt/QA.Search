using Microsoft.Extensions.Logging;
using QA.Search.Admin.Errors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QA.Search.Admin.Models
{
    public abstract class ServiceBase
    {
        protected ILogger Logger { get; }
        public ServiceBase(ILogger logger)
        {
            Logger = logger;
        }
        protected virtual string ErrorMessage { get; } = "Ошибка в работе сервиса";
        protected async Task<TResult> WrapActionAsync<TResult>(Func<Task<TResult>> doIt, string errorMessage = null)
        {
            try
            {
                return await doIt();
            }
            catch (Exception e)
            {
                LogException(e);
                throw new BusinessError(errorMessage ?? ErrorMessage);
            }
        }

        protected TResult WrapAction<TResult>(Func<TResult> doIt, string errorMessage = null)
        {
            try
            {
                return doIt();
            }
            catch (Exception e)
            {
                LogException(e);
                throw new BusinessError(errorMessage ?? ErrorMessage);
            }
        }

        protected void LogException(Exception e)
        {
            var message = $"During work of service {GetType().AssemblyQualifiedName} was thrown exception";
            Logger.LogError(e, message);
        }
    }
}

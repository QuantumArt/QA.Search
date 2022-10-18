using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace QA.Search.Generic.Integration.QP.Interfaces
{
    /// <summary>
    /// Интерфейс для регистрации компонентов кастомных индексаторов
    /// </summary>
    public interface IServiceRegistrator
    {
        /// <summary>
        /// Регистрация компонентов кастомного индексатора
        /// </summary>
        /// <param name="services" <see cref="IServiceCollection"/>/>
        /// <param name="configuration" <see cref="IConfiguration"/> />
        void RegisterServices(IServiceCollection services, IConfiguration configuration);
    }
}

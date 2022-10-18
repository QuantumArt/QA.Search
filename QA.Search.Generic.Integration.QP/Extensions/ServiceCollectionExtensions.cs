using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QA.Search.Generic.DAL.Services;
using QA.Search.Generic.Integration.QP.Infrastructure;
using QA.Search.Generic.Integration.QP.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace QA.Search.Generic.Integration.QP.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Зарегистрировать указанную <see cref="ElasticView{}"/>, если она не имеет <see cref="IgnoreViewAttribute"/>
        /// </summary>
        /// <exception cref="InvalidOperationException" />
        public static void AddElasticView<TView>(this IServiceCollection services)
        {
            Type viewType = typeof(TView);

            if (Attribute.IsDefined(viewType, typeof(IgnoreViewAttribute))) return;

            while (viewType != null && !IsElasticView(viewType))
            {
                viewType = viewType.BaseType;
            }

            if (viewType == null)
            {
                throw new InvalidOperationException($"Type {typeof(TView)} does not implement ElasticView<>");
            }

            services.AddTransient(viewType, typeof(TView));
        }

        private static bool IsElasticView(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ElasticView<>);
        }

        /// <summary>
        /// Зарегистрировать все <see cref="ElasticView{}"/> в проекте, относящиеся к
        /// заданому <typeparamref name="TDataContext"/> и не имеющие <see cref="IgnoreViewAttribute"/>
        /// </summary>
        public static void AddElasticViews<TDataContext>(this IServiceCollection services)
            where TDataContext : GenericDataContext
        {
            IEnumerable<Type> viewTypes = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => !type.IsAbstract &&
                    typeof(ElasticView<TDataContext>).IsAssignableFrom(type) &&
                    !Attribute.IsDefined(type, typeof(IgnoreViewAttribute)));

            foreach (Type viewType in viewTypes)
            {
                services.AddTransient(typeof(ElasticView<TDataContext>), viewType);
            }
        }

        /// <summary>
        /// Загружает все библоириеки кастомных индексаторов и запускает для каждого
        /// метод регистрации его компонентов в DI.
        /// </summary>
        /// <param name="services" <see cref="IServiceCollection" /> />
        /// <param name="configuration" <see cref="IConfiguration"/> />
        /// <param name="assemblies">Список библиотек для загрузки (только имя, без расширения).</param>
        /// <exception cref="DllNotFoundException"></exception>
        public static void AddIndexersByAssemblyNames(this IServiceCollection services, IConfiguration configuration, string[] assemblies)
        {
            foreach (string assembly in assemblies)
            {
                string fileName = $"{assembly}.dll";

                if (!File.Exists(fileName))
                    throw new DllNotFoundException($"Can't find indexer assembly file with name {fileName}.");

                Assembly indexerAssembly = Assembly.LoadFrom(fileName);

                Type type = indexerAssembly.GetTypes().Where(type => typeof(IServiceRegistrator).IsAssignableFrom(type)).Single();

                IServiceRegistrator registrator = (IServiceRegistrator)Activator.CreateInstance(type);

                registrator.RegisterServices(services, configuration);
            }
        }
    }
}

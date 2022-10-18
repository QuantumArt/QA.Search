using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using QA.Search.Generic.DAL.Exceptions;
using QA.Search.Generic.DAL.Services.Configuration;

namespace QA.Search.Generic.DAL.Services.Extensions
{
    public class DbContextTool
    {
        public static DbContextOptions DefaultConnectionOptions<T>() where T : DbContext
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                        .AddJsonFile("appsettings.json")
                        .Build();

            return DefaultConnectionOptions<T>(configuration);
        }
        public static DbContextOptions DefaultConnectionOptions<T>(IConfiguration configuration) where T : DbContext
        {
            ContextConfiguration contextConfiguration = new ContextConfiguration();
            configuration.GetSection(nameof(ContextConfiguration)).Bind(contextConfiguration);

            return DefaultConnectionOptions<T>(contextConfiguration);
        }
        public static DbContextOptions DefaultConnectionOptions<T>(ContextConfiguration contextConfiguration) where T : DbContext
        {
            DbContextOptionsBuilder<T> optionsBuilder = new DbContextOptionsBuilder<T>();

            switch (contextConfiguration.SqlServerType)
            {
                case SqlServerType.MSSQL:
                    optionsBuilder.UseSqlServer(contextConfiguration.ConnectionString);
                    break;
                case SqlServerType.PostgreSQL:
                    optionsBuilder.UseNpgsql(contextConfiguration.ConnectionString);
                    break;
                default:
                    throw new NotSupportedSqlServerTypeException(contextConfiguration.SqlServerType);
            }

            return optionsBuilder.Options;
        }
    }
}

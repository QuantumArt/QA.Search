using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QA.Search.Generic.Integration.Core.Extensions;
using QA.Search.Generic.Integration.Core.Models;
using QA.Search.Generic.Integration.QP.Permissions.Configuration;
using QA.Search.Generic.Integration.QP.Permissions.Interfaces;
using QA.Search.Generic.Integration.QP.Permissions.Markers;
using QA.Search.Generic.Integration.QP.Permissions.Models;
using QA.Search.Generic.Integration.QP.Permissions.Services;

namespace QA.Search.Generic.Integration.QP.Permissions.Extensions
{
    public static class PermissionsServiceExtension
    {
        public static void AddPermissionsService(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<Settings<QpPermissionsMarker>>(config.GetSection("Settings.QP.Permissions"));
            services.Configure<PermissionsConfiguration>(config.GetSection(nameof(PermissionsConfiguration)));
            services.AddDbContext<PermissionsDataContext>(ServiceLifetime.Transient);
            services.AddSingleton<PermissionsLoader>();
            services.AddSingleton<IPermissionService, PermissionService>();
            services.AddScheduledService<ScheduledServiceQPPermissions, Settings<QpPermissionsMarker>, IndexingQpPermissionsContext, QpPermissionsMarker>();
        }
    }
}

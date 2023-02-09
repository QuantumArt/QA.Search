using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace QA.Search.Generic.Integration.QP.Permissions.Interfaces
{
    public interface IPermissionService
    {
        Task<IEnumerable<string>> GetPermissions(
            decimal contentId,
            decimal contetnItemId,
            string abstractName,
            CancellationToken cancellationToken);

        Task<IEnumerable<string>> AbstractItemPermissions(string content, CancellationToken stoppingToken);
    }
}

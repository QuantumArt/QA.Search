using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using QA.Search.Generic.Integration.QP.Permissions.Configuration;
using QA.Search.Generic.Integration.QP.Permissions.Interfaces;
using QA.Search.Generic.Integration.QP.Permissions.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace QA.Search.Generic.Integration.QP.Permissions.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly PermissionsDataContext _context;
        private readonly PermissionsConfiguration _permConfig;

        private IEnumerable<string> _permissions = Array.Empty<string>();
        private const int AbstractItemContentId = 537;

        public PermissionService(PermissionsDataContext context, IOptions<PermissionsConfiguration> permConfig)
        {
            _context = context;
            _permConfig = permConfig.Value;
        }

        public async Task<IEnumerable<string>> GetPermissions(
            decimal contentId,
            decimal contetnItemId,
            string abstractName,
            CancellationToken cancellationToken)
        {
            decimal roleToContentLinkId = await GetRoleToContentLinkId(contentId, cancellationToken);

            string[] roles = await GetItemPermissions(roleToContentLinkId, contetnItemId, cancellationToken);

            return roles.Length != 0 ? roles : await AbstractItemPermissions(abstractName, cancellationToken);
        }

        public async Task<IEnumerable<string>> AbstractItemPermissions(string content, CancellationToken stoppingToken)
        {
            if (_permissions.Any())
            {
                return _permissions;
            }

            decimal roleToContentLinkId = await GetRoleToContentLinkId(AbstractItemContentId, stoppingToken);

            DAL.Models.QPAbstractItem? contentItem = await _context.QPAbstractItems
                .Where(x => x.Name == content)
                .FirstOrDefaultAsync(stoppingToken);

            if (contentItem is null)
            {
                throw new InvalidOperationException($"Can't find [{content}] content id.");
            }

            _permissions = await GetContentPermissions(
                contentItem.ContentItemID,
                contentItem.ParentID,
                roleToContentLinkId,
                stoppingToken);

            return _permissions;
        }

        private async Task<decimal> GetRoleToContentLinkId(decimal contentId, CancellationToken stoppingToken)
        {
            decimal roleContentId = await _context.GetContentIdByDotNetName(nameof(UserRole));

            if (roleContentId == default)
            {
                throw new InvalidOperationException("Can't find content with roles.");
            }

            decimal roleToContentLinkId = await _context.ContentMapping
                    .Where(x => x.RightContentId == roleContentId && x.LeftContentId == contentId)
                    .Select(x => x.LinkId)
                    .FirstOrDefaultAsync(stoppingToken);

            return roleToContentLinkId == default
                ? throw new InvalidOperationException("There is no link between role content and abstract items for permissions fetching.")
                : roleToContentLinkId;
        }

        private async Task<string[]> GetItemPermissions(decimal roleToContentLinkId, decimal indexId, CancellationToken stoppingToken)
        {
            return await _context.ItemMapping
                .Join(_context.Roles, im => im.RightItemId, r => r.ContentItemID, (im, r) => new { im.LinkId, im.LeftItemId, im.Reversed, r.Alias })
                .Where(x => x.LinkId == roleToContentLinkId && x.LeftItemId == indexId && x.Reversed == false)
                .Select(x => x.Alias)
                .ToArrayAsync(stoppingToken);
        }

        private async Task<IEnumerable<string>> GetContentPermissions(
            decimal indexId,
            decimal? parentId,
            decimal roleToContentLinkId,
            CancellationToken stoppingToken)
        {
            decimal? currentId = indexId;
            List<string> permissions = new();

            do
            {
                //Достаём из БД роли для контента по его Id.
                string[] dbRoles = await GetItemPermissions(roleToContentLinkId, indexId, stoppingToken);

                //Если нашли роли, то возвращаем список ролей.
                if (dbRoles != null && dbRoles.Length > 0)
                {
                    permissions.AddRange(dbRoles);
                }

                currentId = parentId;
                //Достаём родительский контент.
                parentId = await _context.QPAbstractItems
                    .Where(x => x.ContentItemID == parentId)
                    .Select(x => x.ParentID)
                    .FirstAsync(stoppingToken);
            }
            while (parentId.HasValue);

            permissions.Add(_permConfig.DefaultRoleAlias);

            return permissions;
        }
    }
}

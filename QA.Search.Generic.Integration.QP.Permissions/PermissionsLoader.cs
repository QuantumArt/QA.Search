using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QA.Search.Generic.Integration.Core.Models;
using QA.Search.Generic.Integration.QP.Permissions.Configuration;
using QA.Search.Generic.Integration.QP.Permissions.Markers;
using QA.Search.Generic.Integration.QP.Permissions.Models;
using QA.Search.Generic.Integration.QP.Permissions.Models.DTO;
using QA.Search.Generic.Integration.QP.Permissions.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace QA.Search.Generic.Integration.QP.Permissions
{
    public class PermissionsLoader
    {
        private readonly PermissionsDataContext _permContext;
        private readonly PermissionsConfiguration _permConfig;
        private readonly ILogger<PermissionsLoader> _logger;
        private readonly IOptions<Settings<QpPermissionsMarker>> _settings;
        private readonly List<ContentInformation> _contentsInfo;
        private readonly List<PermissionCache> _permissionsCache;

        private decimal _roleContentId;
        private decimal _roleToContentLinkId;

        public PermissionsLoader(PermissionsDataContext permContext,
            IOptions<PermissionsConfiguration> permConfig,
            ILogger<PermissionsLoader> logger,
            IOptions<Settings<QpPermissionsMarker>> settingsOptions)
        {
            _permContext = permContext;
            _permConfig = permConfig.Value;
            _logger = logger;
            _settings = settingsOptions;
            _permissionsCache = new List<PermissionCache>();
            _contentsInfo = new List<ContentInformation>();
        }

        public async Task<List<IndexesByRoles>> LoadAsync(CancellationToken stoppingToken)
        {
            try
            {
                _roleContentId = await _permContext.GetContentIdByDotNetName(nameof(UserRole));

                if (_roleContentId == default)
                    throw new InvalidOperationException("Can't find content with roles.");

                _roleToContentLinkId = await _permContext.ContentMapping
                    .Where(x => x.RightContentId == _roleContentId)
                    .Select(x => x.LinkId)
                    .FirstOrDefaultAsync(stoppingToken);

                if (_roleToContentLinkId == default)
                    throw new InvalidOperationException("There is no link between role content and abstract items for permissions fetching.");

                _contentsInfo.AddRange(await _permContext.QPAbstractItems
                    .Where(x => _permConfig.QpAbstractItems.Contains(x.Name))
                    .Select(x => new ContentInformation
                    {
                        ContentItemId = x.ContentItemID,
                        ContentName = x.Name,
                        ParentId = x.ParentID
                    })
                    .ToListAsync(stoppingToken));

                if (_contentsInfo.Count == 0)
                    throw new InvalidOperationException("There is no contents to find permissions for.");

                foreach (ContentInformation contentInfo in _contentsInfo)
                {
                    contentInfo.Roles = await GetContentPermissions(contentInfo.ContentItemId, contentInfo.ParentId, stoppingToken);
                }

                _contentsInfo.ForEach(x => x.IndexName = _settings.Value.AliasPrefix + x.ContentName);

                List<string> roles = _contentsInfo
                    .SelectMany(x => x.Roles)
                    .Distinct()
                    .ToList();

                List<IndexesByRoles> permissions = new List<IndexesByRoles>();

                foreach (string role in roles)
                {
                    permissions.Add(new IndexesByRoles()
                    {
                        Role = role.ToLowerInvariant(),
                        Indexes = _contentsInfo
                        .Where(x => x.Roles.Contains(role))
                        .Select(x => x.IndexName)
                        .ToArray()
                    });
                }

                return permissions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while loading permissions.");
                throw;
            }
            finally
            {
                _roleContentId = default;
                _roleToContentLinkId = default;
                _contentsInfo.Clear();
                _contentsInfo.TrimExcess();
                _permissionsCache.Clear();
                _permissionsCache.TrimExcess();
            }
        }

        /// <summary>
        /// Метод для поиска разрешений на раздел.
        /// Ищет разрешения для конкретного контента.
        /// Если на контент нет разрешения, то достаёт родительский конетент и вызывает сам себя для поиска разрешений родительского контента (венимание, рекурсия).
        /// Если у родителя верхнего уровня нет контента, то считаем, что контент доступен всем.
        /// </summary>
        /// <param name="indexId">Идентификатор контента</param>
        /// <param name="parentId">Идентификатор родительского контента</param>
        /// <returns></returns>
        private async Task<List<string>> GetContentPermissions(decimal indexId, decimal? parentId, CancellationToken stoppingToken)
        {
            //Проверяем есть ли разрешения в локальном кеше, чтоб если у контента схожая структура наследования и не заданы разрешения
            //не пришлось заново проходить рекурсией по всем родителям.
            List<string>? cachedRoles = _permissionsCache
                .Where(x => x.ContentId == indexId)
                .Select(x => x.Roles)
                .FirstOrDefault();

            //Если в кеше есть роли, то обновляем все роли в кеше для новых кешированных контентов и возвращаем список ролей.
            if (cachedRoles != null && cachedRoles.Count > 0)
            {
                UpdateCacheRoles(cachedRoles);
                return cachedRoles;
            }

            //Если в кеше нет - добавляем контент в кеш.
            _permissionsCache.Add(new PermissionCache() { ContentId = indexId });

            //Достаём из БД роли для контента по его Id.
            List<string>? dbRoles = await _permContext.ItemMapping
                .Join(_permContext.Roles, im => im.RightItemId, r => r.ContentItemID, (im, r) => new { im.LinkId, im.LeftItemId, im.Reversed, r.Alias })
                .Where(x => x.LinkId == _roleToContentLinkId && x.LeftItemId == indexId && x.Reversed == false)
                .Select(x => x.Alias)
                .ToListAsync(stoppingToken);

            //Если нашли роли, то обновляем все роли в кеше для новых кешированных контентов и возвращаем список ролей.
            if (dbRoles != null && dbRoles.Count > 0)
            {
                UpdateCacheRoles(dbRoles);
                return dbRoles;
            }

            //Если у контента нет родителя (корневой контент), то берём из конфига разрешения по умолчанию,
            //обновляем все роли в кеше для новых кешированных контентов и возвращаем список ролей по умолчанию.
            if (parentId is null)
            {
                List<string> defaultReaderRole = new List<string> { _permConfig.DefaultRoleAlias };
                UpdateCacheRoles(defaultReaderRole);
                return defaultReaderRole;
            }

            //Достаём родительский контент.
            decimal? newParentId = await _permContext.QPAbstractItems
                .Where(x => x.ContentItemID == parentId)
                .Select(x => x.ParentID)
                .FirstAsync(stoppingToken);

            //Пытаемся получить список ролей для родительского контента. (Внимание, рекурсия!)
            List<string> resultRoles = await GetContentPermissions((decimal)parentId, newParentId, stoppingToken);
            return resultRoles;
        }

        /// <summary>
        /// Обновляем список ролей для всех контентов в кеше у которых роли не заполнены.
        /// Если мы прошлись скажем по 10-ти контентам через родителя каждого контента и в итоге нашли какие-то роли,
        /// то у всей цепочки будут наследованные роли. Проставляем сразу всем идентичный список ролей.
        /// </summary>
        /// <param name="roles">Список ролей.</param>
        private void UpdateCacheRoles(List<string> roles)
        {
            _permissionsCache
                .Where(x => x.Roles.Count == 0)
                .Select(x => x)
                .ToList()
                .ForEach(x => x.Roles = roles);
        }
    }
}

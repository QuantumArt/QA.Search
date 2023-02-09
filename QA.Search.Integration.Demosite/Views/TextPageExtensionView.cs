using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QA.Search.Generic.DAL.Services;
using QA.Search.Generic.DAL.Services.Configuration;
using QA.Search.Generic.DAL.Services.Interfaces;
using QA.Search.Generic.Integration.Core.Models;
using QA.Search.Generic.Integration.Core.Models.DTO;
using QA.Search.Generic.Integration.QP.Infrastructure;
using QA.Search.Integration.Demosite.Models;
using QA.Search.Integration.Demosite.Services;

namespace QA.Search.Integration.Demosite.Views
{
    public class TextPageExtensionView : ElasticView<GenericDataContext>
    {
        private readonly IUrlService<DemositeDataContext> _urlService;
        private readonly DemositeSettings _demositeSettings;

        public override string IndexName => nameof(DemositeDataContext.TextPages);

        public override string ViewName => nameof(TextPageExtensionView);

        public TextPageExtensionView(DemositeDataContext context, ILogger logger,
            IOptions<ContextConfiguration> contextConfigurationOption,
            IOptions<GenericIndexSettings> genericIndexSettingsOption,
            IUrlService<DemositeDataContext> urlService,
            IOptions<DemositeSettings> demositeSettings)
            : base(context, logger, contextConfigurationOption, genericIndexSettingsOption)
        {
            _urlService = urlService;
            _demositeSettings = demositeSettings.Value;
        }

        protected override IQueryable Query => ((DemositeDataContext)Db).TextPages;

        public override async Task<JObject[]> LoadAsync(LoadParameters loadParameters, CancellationToken token)
        {
            var posts = await ((DemositeDataContext)Db).TextPages
                .Where(x => x.ItemID != null
                    && !_demositeSettings.IgnoreContentItemIds.Contains(x.ContentItemID)
                    && x.ContentItemID > loadParameters.FromID
                    && x.Modified > loadParameters.FromDate)
                .OrderBy(x => x.ContentItemID)
                .Select(x => new TextPageDto()
                {
                    ContentItemId = x.ContentItemID,
                    Text = x.Text,
                    ItemID = x.ItemID,
                    Description = x.Text == null
                        ? "Подробности читайте на странице по ссылке."
                        : $"{x.Text.Substring(0, 500)}..."
                })
                .Take(loadParameters.ViewParameters.BatchSize)
                .ToListAsync(token);

            JObject[] documents = new JObject[posts.Count];

            for (int post = 0; post < posts.Count; post++)
            {
                documents[post] = JObject.Parse(JsonConvert.SerializeObject(posts[post]));
                string url = await _urlService.GetUrlToPageByIdAsync(posts[post].ItemID, token);
                documents[post][genericIndexSettings.SearchUrlField] = _urlService.UrlToJArray(genericIndexSettings.SearchUrlField, url);
                documents[post]["title"] = await ((DemositeDataContext)Db).QPAbstractItems
                    .Where(a => a.ContentItemID == posts[post].ItemID)
                    .Select(s => s.Title)
                    .FirstOrDefaultAsync(token);
            }

            return documents;
        }

        public override async Task<int> CountAsync(DateTime fromDate, CancellationToken token)
        {
            return await ((DemositeDataContext)Db).TextPages
                .Where(x => x.ItemID != null
                    && !_demositeSettings.IgnoreContentItemIds.Contains(x.ContentItemID))
                .CountAsync(token);
        }
    }
}

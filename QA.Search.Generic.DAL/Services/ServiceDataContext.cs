using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using QA.Search.Generic.DAL.Exceptions;
using QA.Search.Generic.DAL.Models;
using QA.Search.Generic.DAL.Services.Configuration;
using QA.Search.Generic.DAL.Services.Extensions;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace QA.Search.Generic.DAL.Services
{
    public class ServiceDataContext : DbContext
    {
        private const string
            TableLive = "content_{0}_new",
            TableStage = "content_{0}_united_new",
            TableLiveFiltered = "content_{0}_live_new",
            TableStageFiltered = "content_{0}_stage_new";

        private readonly ContextConfiguration _contextConfiguration;

        public DbSet<Content> Contents { get; set; }

        public ServiceDataContext(IOptions<ContextConfiguration> contextConfigurationOption)
            : base(DbContextTool.DefaultConnectionOptions<ServiceDataContext>(contextConfigurationOption.Value))
        {
            _contextConfiguration = contextConfigurationOption.Value;

            ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        public string GetTableNameFromDotNetName(string dotNetName)
        {
            Content content = Contents
                .Where(x => x.DotNetName.Equals(dotNetName))
                .FirstOrDefault();

            if (content is null)
                throw new NotFoundTableNameException(dotNetName);

            return GetTableName(content);
        }

        public async Task<decimal> GetContentIdByDotNetName(string dotNetName)
        {
            return await Contents
                .Where(x => x.DotNetName == dotNetName)
                .Select(x => x.ContentID)
                .FirstOrDefaultAsync();
        }

        public string GetTableNameFromContentName(string contentName)
        {
            Content content = Contents
                .Where(x => x.ContentName.Equals(contentName))
                .FirstOrDefault();

            if (content is null)
                throw new NotFoundTableNameException(contentName);

            return GetTableName(content);
        }

        private string GetTableName(Content content)
        {
            switch (_contextConfiguration.ContentAccess)
            {
                case ContentAccess.Live:
                    return string.Format(content.UseDefaultFiltration ? TableLiveFiltered : TableLive, content.ContentID);
                case ContentAccess.Stage:
                    return string.Format(content.UseDefaultFiltration ? TableStageFiltered : TableStage, content.ContentID);
                case ContentAccess.StageNoDefaultFiltration:
                    return string.Format(TableStage, content.ContentID);
                default:
                    throw new NotSupportedContentAccessException(_contextConfiguration.ContentAccess);
            }
        }
    }
}

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
            TABLE_LIVE = "content_{0}_new",
            TABLE_STAGE = "content_{0}_united_new",
            TABLE_LIVE_FILTERED = "content_{0}_live_new",
            TABLE_STAGE_FILTERED = "content_{0}_stage_new";

        private const string
            M2M_TABLE = "item_link_{0}",
            M2M_TABLE_ASYNC = $"{M2M_TABLE}_async",
            M2M_TABLE_REVERSE = $"{M2M_TABLE}_rev",
            M2M_TABLE_ASYNC_REVERSE = $"{M2M_TABLE}_async_rev";

        private readonly ContextConfiguration _contextConfiguration;

        public DbSet<Content> Contents { get; set; }
        public DbSet<ContentItem> ContentItems { get; set; }
        public DbSet<Item2Item> Item2Item { get; set; }
        public DbSet<ContentAttribute> ContentAttributes { get; set; }
        public DbSet<AttributeType> AttributeTypes { get; set; }
        public DbSet<Content2Content> Content2Content { get; set; }

        public ServiceDataContext(IOptions<ContextConfiguration> contextConfigurationOption)
            : base(DbContextTool.DefaultConnectionOptions<ServiceDataContext>(contextConfigurationOption.Value))
        {
            _contextConfiguration = contextConfigurationOption.Value;

            ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Item2Item>()
                .HasKey(c => new { c.LinkId, c.LeftItemId, c.RightItemId });

            base.OnModelCreating(modelBuilder);
        }

        public string GetTableNameFromDotNetName(string dotNetName)
        {
            Content content = Contents
                .Where(x => x.DotNetName.Equals(dotNetName))
                .FirstOrDefault();

            return content is null ? throw new NotFoundTableNameException(dotNetName) : GetTableName(content);
        }

        public Task<decimal> GetContentIdByDotNetName(string dotNetName)
        {
            return Contents
                .Where(x => x.DotNetName == dotNetName)
                .Select(x => x.ContentID)
                .FirstOrDefaultAsync();
        }


        public string GetTableNameFromContentName(string contentName)
        {
            Content content = Contents
                .Where(x => x.ContentName.Equals(contentName))
                .FirstOrDefault();

            return content is null ? throw new NotFoundTableNameException(contentName) : GetTableName(content);
        }

        private string GetTableName(Content content)
        {
            return _contextConfiguration.ContentAccess switch
            {
                ContentAccess.Live => string.Format(content.UseDefaultFiltration ? TABLE_LIVE_FILTERED : TABLE_LIVE, content.ContentID),
                ContentAccess.Stage => string.Format(content.UseDefaultFiltration ? TABLE_STAGE_FILTERED : TABLE_STAGE, content.ContentID),
                ContentAccess.StageNoDefaultFiltration => string.Format(TABLE_STAGE, content.ContentID),
                _ => throw new NotSupportedContentAccessException(_contextConfiguration.ContentAccess),
            };
        }

        public string GetM2MIntermediateTableName(string dotNetNameOrContentName, string attributeName, bool reverse)
        {
            decimal itemLinkNumber = GetM2MTableId(dotNetNameOrContentName, attributeName);

            string tableTemplate = GetIntermediateTableTemplate(reverse, false);

            return string.Format(tableTemplate, itemLinkNumber);
        }

        public string GetM2MIntermediateTableNameByRelationTables(string dotNetNameOrContentNameLeft, string dotNetNameOrContentNameRight, bool reverse)
        {
            decimal itemLinkNumber = GetM2MTableIdByTablesRelation(dotNetNameOrContentNameLeft, dotNetNameOrContentNameRight);

            string tableTemplate = GetIntermediateTableTemplate(reverse, false);

            return string.Format(tableTemplate, itemLinkNumber);
        }
        private static string GetIntermediateTableTemplate(bool reverse, bool splitted)
        {
            return reverse ? GetM2MIntermediateReverseTableTemplate(splitted) : GetM2MIntermediateTableTemplate(splitted);
        }

        private decimal GetM2MTableId(string dotNetNameOrContentName, string attributeName)
        {
            Content content = GetContentByDotNetNameOrContentName(dotNetNameOrContentName);

            return Convert.ToDecimal(ContentAttributes
                 .Where(x => x.ContentID == content.ContentID && x.AttributeName == attributeName)
                 .Select(x => x.DefaultValue)
                 .FirstOrDefault());
        }

        private Content GetContentByDotNetNameOrContentName(string dotNetNameOrContentName)
        {
            Content content = Contents
                .Where(x => x.DotNetName == dotNetNameOrContentName || x.ContentName == dotNetNameOrContentName)
                .FirstOrDefault();

            return content is null ? throw new NotFoundTableNameException(dotNetNameOrContentName) : content;
        }

        private decimal GetM2MTableIdByTablesRelation(string dotNetNameOrContentNameLeft, string dotNetNameOrContentNameRight)
        {
            Content contentLeft = GetContentByDotNetNameOrContentName(dotNetNameOrContentNameLeft);
            Content contentRight = GetContentByDotNetNameOrContentName(dotNetNameOrContentNameRight);

            Content2Content content2Content = Content2Content
                .Include(x => x.LeftContent)
                .Include(x => x.RightContent)
                .Where(x => x.LeftContent.ContentID == contentLeft.ContentID && x.RightContent.ContentID == contentRight.ContentID)
                .AsSplitQuery()
                .FirstOrDefault();

            return Convert.ToDecimal(ContentAttributes
                .Include(x => x.Content2Content.LeftContent)
                .Include(x => x.Content2Content.RightContent)
                .Where(x => x.Content2Content.LeftContent.ContentID == contentLeft.ContentID
                    && x.Content2Content.RightContent.ContentID == contentRight.ContentID)
                .AsSplitQuery()
                .Select(x => x.DefaultValue)
                .FirstOrDefault());
        }

        private static string GetM2MIntermediateTableTemplate(bool splitted)
        {
            return splitted switch
            {
                true => M2M_TABLE_ASYNC,
                false => M2M_TABLE
            };
        }
        private static string GetM2MIntermediateReverseTableTemplate(bool splitted)
        {
            return splitted switch
            {
                true => M2M_TABLE_ASYNC_REVERSE,
                false => M2M_TABLE_REVERSE
            };
        }
    }
}

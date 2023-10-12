using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Options;
using QA.Search.Generic.DAL.Models;
using QA.Search.Generic.DAL.Services.Configuration;
using QA.Search.Generic.DAL.Services.Extensions;
using System;
using System.Threading.Tasks;

namespace QA.Search.Generic.DAL.Services
{
    public abstract class GenericDataContext : DbContext
    {
        private readonly ServiceDataContext _serviceDataContext;
        private readonly ContextConfiguration _contextConfiguration;

        public DbSet<StatusType> StatusTypes { get; set; }
        public DbSet<QPAbstractItem> QPAbstractItems { get; set; }
        public DbSet<StartPageExtension> StartPage { get; set; }

        public DbSet<AppSetting> AppSettings { get; set; }

        public GenericDataContext(ServiceDataContext serviceDataContext, IOptions<ContextConfiguration> contextConfigurationOption)
            : base(DbContextTool.DefaultConnectionOptions<GenericDataContext>(contextConfigurationOption.Value))
        {
            _serviceDataContext = serviceDataContext;
            _contextConfiguration = contextConfigurationOption.Value;

            ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        public string GetTableFullName(Type type)
        {
            IEntityType entityType = Model.FindEntityType(type);
            string schema = entityType.GetSchema() ?? _contextConfiguration.DefaultSchemeName;
            string tableName = entityType.GetTableName();

            if (tableName.EndsWith("_new"))
            {
                tableName = tableName[..^"_new".Length];
            }

            return $"{schema}.{tableName}";
        }

        public Task<decimal> GetContentIdByDotNetName(string dotNetName)
        {
            return string.IsNullOrWhiteSpace(dotNetName)
                ? throw new ArgumentNullException(nameof(dotNetName))
                : _serviceDataContext.GetContentIdByDotNetName(dotNetName);
        }

        public string GetTableNameFromDotNetName(string dotNetName)
        {
            return string.IsNullOrWhiteSpace(dotNetName)
                ? throw new ArgumentNullException(nameof(dotNetName))
                : _serviceDataContext.GetTableNameFromDotNetName(dotNetName);
        }
        public string GetTableNameFromContentName(string contentName)
        {
            return string.IsNullOrWhiteSpace(contentName)
                ? throw new ArgumentNullException(nameof(contentName))
                : _serviceDataContext.GetTableNameFromContentName(contentName);
        }

        public string GetM2MIntermediateTableName(string dotNetNameOrContentName, string attributeName, bool reverse)
        {
            if (string.IsNullOrWhiteSpace(dotNetNameOrContentName))
                throw new ArgumentNullException(nameof(dotNetNameOrContentName));

            if (string.IsNullOrWhiteSpace(attributeName))
                throw new ArgumentNullException(nameof(attributeName));

            return _serviceDataContext.GetM2MIntermediateTableName(dotNetNameOrContentName, attributeName, reverse);
        }
        public string GetM2MIntermediateTableNameByRelationTables(string dotNetNameOrContentNameLeft, string dotNetNameOrContentNameRight, bool reverse)
        {
            if (string.IsNullOrWhiteSpace(dotNetNameOrContentNameLeft))
                throw new ArgumentNullException(nameof(dotNetNameOrContentNameLeft));

            if (string.IsNullOrWhiteSpace(dotNetNameOrContentNameRight))
                throw new ArgumentNullException(nameof(dotNetNameOrContentNameRight));

            return _serviceDataContext.GetM2MIntermediateTableNameByRelationTables(dotNetNameOrContentNameLeft, dotNetNameOrContentNameRight, reverse);
        }

        public abstract void ModelCreating(ModelBuilder modelBuilder);

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<QPAbstractItem>()
                .ToTable(_serviceDataContext.GetTableNameFromDotNetName(nameof(QPAbstractItem)));

            modelBuilder.Entity<QPAbstractItem>()
                .HasOne(x => x.Parent)
                .WithMany(x => x.Children)
                .HasForeignKey(fk => fk.ParentID);

            modelBuilder.Entity<QPAbstractItem>()
                .HasOne(x => x.Discriminator)
                .WithMany(d => d.AbstractItems)
                .HasForeignKey(fk => fk.DiscriminatorID);

            modelBuilder.Entity<QPDiscriminator>()
                .ToTable(_serviceDataContext.GetTableNameFromDotNetName(nameof(QPDiscriminator)));

            modelBuilder.Entity<StartPageExtension>()
                .ToTable(GetTableNameFromContentName(nameof(StartPageExtension)));

            modelBuilder.Entity<AppSetting>()
                .ToTable("app_settings");

            ModelCreating(modelBuilder);
        }
    }
}

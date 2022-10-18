using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Options;
using QA.Search.Generic.DAL.Models;
using QA.Search.Generic.DAL.Services.Configuration;
using QA.Search.Generic.DAL.Services.Extensions;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace QA.Search.Generic.DAL.Services
{
    public abstract class GenericDataContext : DbContext
    {
        private readonly ServiceDataContext _serviceDataContext;
        private readonly ContextConfiguration _contextConfiguration;

        public DbSet<StatusType> StatusTypes { get; set; }
        public DbSet<QPAbstractItem> QPAbstractItems { get; set; }

        public GenericDataContext(ServiceDataContext serviceDataContext, IOptions<ContextConfiguration> contextConfigurationOption)
            : base(DbContextTool.DefaultConnectionOptions<GenericDataContext>(contextConfigurationOption.Value))
        {
            _serviceDataContext = serviceDataContext;
            _contextConfiguration = contextConfigurationOption.Value;
        }

        public string GetTableFullName(Type type)
        {
            IEntityType entityType = Model.FindEntityType(type);
            string schema = entityType.GetSchema() ?? _contextConfiguration.DefaultSchemeName;
            string tableName = entityType.GetTableName();

            if (tableName.EndsWith("_new"))
                tableName = tableName.Substring(0, tableName.Length - "_new".Length);

            return $"{schema}.{tableName}";
        }

        public async Task<decimal> GetContentIdByDotNetName(string dotNetName)
        {
            if (string.IsNullOrWhiteSpace(dotNetName))
                throw new ArgumentNullException(nameof(dotNetName));

            return await _serviceDataContext.GetContentIdByDotNetName(dotNetName);
        }

        public string GetTableNameFromDotNetName(string dotNetName)
        {
            if (string.IsNullOrWhiteSpace(dotNetName))
                throw new ArgumentNullException(nameof(dotNetName));

            return _serviceDataContext.GetTableNameFromDotNetName(dotNetName);
        }
        public string GetTableNameFromContentName(string contentName)
        {
            if (string.IsNullOrWhiteSpace(contentName))
                throw new ArgumentNullException(nameof(contentName));

            return _serviceDataContext.GetTableNameFromContentName(contentName);
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

            ModelCreating(modelBuilder);
        }
    }
}

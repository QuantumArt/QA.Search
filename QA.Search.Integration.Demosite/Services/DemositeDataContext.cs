using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Options;
using QA.Search.Generic.DAL.Services;
using QA.Search.Generic.DAL.Services.Configuration;
using QA.Search.Integration.Demosite.Models;

namespace QA.Search.Integration.Demosite.Services
{
    public class DemositeDataContext : GenericDataContext
    {
        public DbSet<NewsPost> NewsPosts { get; set; } = null!;
        public DbSet<TextPageExtension> TextPages { get; set; } = null!;
        public DbSet<NewsCategory> NewsCategories { get; set; } = null!;

        public DemositeDataContext(
            ServiceDataContext serviceDataContext,
            IOptions<ContextConfiguration> contextConfigurationOption)
            : base(serviceDataContext, contextConfigurationOption)
        {

        }

        public override void ModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<NewsPost>()
                .ToTable(GetTableNameFromDotNetName(nameof(NewsPost)));

            modelBuilder.Entity<TextPageExtension>()
                .ToTable(GetTableNameFromContentName(nameof(TextPageExtension)));

            modelBuilder.Entity<NewsCategory>()
                .ToTable(GetTableNameFromDotNetName(nameof(NewsCategory)));

            ValueConverter<DateTime, DateTime> dateTimeConverter = new(
                v => DateTime.SpecifyKind(v, DateTimeKind.Utc), v => v);

            foreach (Microsoft.EntityFrameworkCore.Metadata.IMutableEntityType entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (Microsoft.EntityFrameworkCore.Metadata.IMutableProperty property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
                    {
                        property.SetValueConverter(dateTimeConverter);
                    }
                }
            }
        }
    }
}

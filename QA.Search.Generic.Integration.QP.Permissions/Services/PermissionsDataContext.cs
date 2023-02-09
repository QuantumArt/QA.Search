using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using QA.Search.Generic.DAL.Services;
using QA.Search.Generic.DAL.Services.Configuration;
using QA.Search.Generic.Integration.QP.Permissions.Models;

namespace QA.Search.Generic.Integration.QP.Permissions.Services
{
    public class PermissionsDataContext : GenericDataContext
    {
        public DbSet<UserRole> Roles { get; set; } = null!;
        public DbSet<ContentToContent> ContentMapping { get; set; } = null!;
        public DbSet<ItemToItem> ItemMapping { get; set; } = null!;

        public PermissionsDataContext(ServiceDataContext serviceDataContext, IOptions<ContextConfiguration> contextConfigurationOption)
            : base(serviceDataContext, contextConfigurationOption) { }

        public override void ModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserRole>()
                .ToTable(GetTableNameFromDotNetName(nameof(UserRole)));

            modelBuilder.Entity<ContentToContent>()
                .ToTable("content_to_content");

            modelBuilder.Entity<ItemToItem>()
                .ToTable("item_to_item")
                .HasKey(iti => new { iti.LinkId, iti.LeftItemId, iti.RightItemId });
        }
    }
}

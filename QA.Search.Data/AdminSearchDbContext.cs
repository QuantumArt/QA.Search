using Microsoft.EntityFrameworkCore;
using QA.Search.Data.Models;

namespace QA.Search.Data
{
    public class AdminSearchDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<ResetPasswordRequest> ResetPasswordRequests { get; set; }
        public DbSet<ReindexTask> ReindexTasks { get; set; }

        public AdminSearchDbContext(DbContextOptions<AdminSearchDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder mb)
        {
            mb.HasDefaultSchema("admin");

            mb.Entity<ReindexTask>()
                .HasKey(nameof(ReindexTask.SourceIndex), nameof(ReindexTask.DestinationIndex));
            mb.Entity<ReindexTask>()
                .UseXminAsConcurrencyToken();

            mb.Entity<User>(u =>
            {
                u.Property(p => p.Id)
                    .UseIdentityAlwaysColumn()
                    .HasIdentityOptions(startValue: 1);

                u.HasData(new User(1, "admin.search@quantumart.ru", "StrongPass1234", UserRole.Admin));
            });
        }
    }
}
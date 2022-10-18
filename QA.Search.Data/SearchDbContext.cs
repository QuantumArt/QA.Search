using Microsoft.EntityFrameworkCore;
using QA.Search.Data.Models;

namespace QA.Search.Data
{
    public class SearchDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<ResetPasswordRequest> ResetPasswordRequests { get; set; }
        public DbSet<ReindexTask> ReindexTasks { get; set; }

        public SearchDbContext(DbContextOptions<SearchDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder mb)
        {
            mb.Entity<User>()
                .ToTable("Users", schema: "admin");

            mb.Entity<ResetPasswordRequest>()
                .ToTable("ResetPasswordRequests", schema: "admin")
                .HasOne(e => e.User)
                .WithMany()
                .HasForeignKey("UserId");

            mb.Entity<ReindexTask>()
                .ToTable("ReindexTasks", schema: "admin")
                .HasKey(nameof(ReindexTask.SourceIndex), nameof(ReindexTask.DestinationIndex));
        }
    }
}
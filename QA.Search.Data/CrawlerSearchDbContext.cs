using Microsoft.EntityFrameworkCore;
using QA.Search.Data.Models;

namespace QA.Search.Data
{
    public class CrawlerSearchDbContext : DbContext
    {
        public DbSet<Domain> Domains { get; set; }
        public DbSet<DomainGroup> DomainGroups { get; set; }
        public DbSet<Link> Links { get; set; }
        public DbSet<Route> Routes { get; set; }

        public CrawlerSearchDbContext(DbContextOptions<CrawlerSearchDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder mb)
        {
            mb.HasDefaultSchema("crawler");

            mb.Entity<Link>(l =>
            {
                l.HasIndex(p => p.NextIndexingUtc).IncludeProperties(p => new { p.Url, p.Version, p.IsActive });
            });
        }
    }
}

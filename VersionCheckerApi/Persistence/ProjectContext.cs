using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using VersionCheckerApi.Persistence.Models;

namespace VersionCheckerApi.Persistence
{
    public class ProjectContext : DbContext
    {
        public ProjectContext(DbContextOptions<ProjectContext> options) : base(options)
        {
        }

        public DbSet<Project> Projects { get; set; } = null!;
        public DbSet<Module> Modules { get; set; } = null!;
        public DbSet<Package> Packages { get; set; } = null!;
        public DbSet<Actionable> Actionables { get; set; } = null!;
        public DbSet<Pipeline> Pipelines { get; set; } = null!;

        public IQueryable<Project> FullProjects()
        {
            return Projects
                .Include(p => p.Pipelines)
                .Include(p => p.Actionables)
                .Include(p => p.Modules)
                .ThenInclude(m => m.Packages)
                .AsSplitQuery();
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            var splitStringConverter = new ValueConverter<List<string>, string>(
                v => string.Join(";", v), v => v.Split(new[] { ';' }).ToList());

            builder.Entity<Package>().Property(nameof(Package.Tags)).HasConversion(splitStringConverter);

            builder.Entity<Package>().HasKey(t => new { t.Name, t.Version, t.Type });

            base.OnModelCreating(builder);
        }
    }
}

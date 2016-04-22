using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using Microsoft.AspNet.Identity.EntityFramework;

namespace WebApiSeed.Models
{
    public class AppDbContext : IdentityDbContext<User>
    {
        public AppDbContext() : base("AppDbContext") { }

        public DbSet<AppSetting> AppSettings { get; set; }
        public DbSet<Profile> Profiles { get; set; }
        public DbSet<ResetRequest> ResetRequests { get; set; }
        public DbSet<EmailOutboxEntry> EmailOutboxEntries { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //Database.SetInitializer(new MigrateDatabaseToLatestVersion<AppDbContext, Migrations.Configuration>());
            base.OnModelCreating(modelBuilder);
        }

        public override int SaveChanges()
        {
            foreach (var entry in ChangeTracker.Entries()
                .Where(x => x.State == EntityState.Added)
                .Select(x => x.Entity)
                .OfType<IAuditable>())
            {
                entry.CreatedAt = DateTime.UtcNow;
                entry.ModifiedAt = DateTime.UtcNow;
            }

            foreach (var entry in ChangeTracker.Entries()
                .Where(x => x.State == EntityState.Modified)
                .Select(x => x.Entity)
                .OfType<IAuditable>())
            {
                entry.ModifiedAt = DateTime.UtcNow;
            }
            return base.SaveChanges();
        }
    }
}
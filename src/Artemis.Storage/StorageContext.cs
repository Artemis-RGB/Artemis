using Artemis.Storage.Entities;
using Microsoft.EntityFrameworkCore;

namespace Artemis.Storage
{
    internal class StorageContext : DbContext
    {
        internal DbSet<Profile> Profiles { get; set; }
        internal DbSet<Setting> Settings { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=Storage.db");
        }

        #region Overrides of DbContext

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Composite keys must be configured with fluent API in Core
            modelBuilder.Entity<Layer>().HasKey(l => new {l.ProfileId, l.Name});

            base.OnModelCreating(modelBuilder);
        }

        #endregion
    }
}

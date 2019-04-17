using Artemis.Storage.Entities;
using Microsoft.EntityFrameworkCore;

namespace Artemis.Storage
{
    public class StorageContext : DbContext
    {
        public DbSet<ProfileEntity> Profiles { get; set; }
        public DbSet<SettingEntity> Settings { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=Storage.db");
            SQLitePCL.Batteries.Init();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SettingEntity>().HasKey(s => new {s.Name, s.PluginGuid});
            base.OnModelCreating(modelBuilder);
        }
    }
}
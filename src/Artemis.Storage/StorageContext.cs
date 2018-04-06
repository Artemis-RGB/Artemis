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
        }
    }
}
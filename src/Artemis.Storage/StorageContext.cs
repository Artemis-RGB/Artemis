using Artemis.Storage.Entities;
using Microsoft.EntityFrameworkCore;

namespace Artemis.Storage
{
    internal class StorageContext : DbContext
    {
        internal DbSet<ProfileEntity> Profiles { get; set; }
        internal DbSet<SettingEntity> Settings { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=Storage.db");
        }
    }
}
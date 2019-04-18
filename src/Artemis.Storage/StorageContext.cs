using System;
using System.IO;
using System.Reflection;
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
#if DEBUG
            var dbLocation = Path.GetFullPath(Path.Combine(Assembly.GetEntryAssembly().Location, @"..\..\..\..\Artemis.Storage\Storage.db"));
#else
            var dbLocation = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\Artemis\\Storage.db";
#endif
            optionsBuilder.UseSqlite("Data Source=" + dbLocation);

            // Requires Microsoft.Data.Sqlite in the startup project
            SQLitePCL.Batteries.Init();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SettingEntity>().HasKey(s => new {s.Name, s.PluginGuid});
            base.OnModelCreating(modelBuilder);
        }
    }
}
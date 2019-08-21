using System;
using System.IO;
using Artemis.Storage.Entities;
using Microsoft.EntityFrameworkCore;
using SQLitePCL;

namespace Artemis.Storage
{
    public class StorageContext : DbContext
    {
        public DbSet<ProfileEntity> Profiles { get; set; }
        public DbSet<SettingEntity> Settings { get; set; }
        public DbSet<PluginSettingEntity> PluginSettings { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // ReSharper disable once RedundantAssignment - Used if not debugging
            var dbLocation = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Artemis\\Storage.db");
            #if DEBUG
            dbLocation = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\Artemis.Storage\Storage.db"));
            #endif

            optionsBuilder.UseSqlite("Data Source=" + dbLocation);

            // Requires Microsoft.Data.Sqlite in the startup project
            Batteries.Init();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SettingEntity>().HasKey(s => s.Name);
            modelBuilder.Entity<PluginSettingEntity>().HasKey(s => new {s.Name, s.PluginGuid});
            base.OnModelCreating(modelBuilder);
        }
    }
}
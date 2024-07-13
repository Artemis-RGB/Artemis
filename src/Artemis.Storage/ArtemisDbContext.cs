using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using Artemis.Storage.Entities.General;
using Artemis.Storage.Entities.Plugins;
using Artemis.Storage.Entities.Profile;
using Artemis.Storage.Entities.Surface;
using Artemis.Storage.Entities.Workshop;
using Microsoft.EntityFrameworkCore;

namespace Artemis.Storage;

public class ArtemisDbContext : DbContext
{
    public DbSet<DeviceEntity> Devices => Set<DeviceEntity>();
    public DbSet<EntryEntity> Entries => Set<EntryEntity>();
    public DbSet<PluginEntity> Plugins => Set<PluginEntity>();
    public DbSet<PluginFeatureEntity> PluginFeatures => Set<PluginFeatureEntity>();
    public DbSet<PluginSettingEntity> PluginSettings => Set<PluginSettingEntity>();
    public DbSet<ProfileCategoryEntity> ProfileCategories => Set<ProfileCategoryEntity>();
    public DbSet<ProfileContainerEntity> ProfileContainers => Set<ProfileContainerEntity>();
    public DbSet<ReleaseEntity> Releases => Set<ReleaseEntity>();

    public string DataFolder { get; set; } = string.Empty;
    public JsonSerializerOptions JsonSerializerOptions { get; set; } = JsonSerializerOptions.Default;

    /// <inheritdoc />
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite($"Data Source={Path.Combine(DataFolder, "artemis.db")}");
    }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DeviceEntity>()
            .OwnsOne(d => d.InputIdentifiers, builder => builder.ToJson())
            .OwnsOne(d => d.InputMappings, builder => builder.ToJson());

        modelBuilder.Entity<EntryEntity>()
            .Property(e => e.Metadata)
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonSerializerOptions),
                v => JsonSerializer.Deserialize<Dictionary<string, JsonNode>>(v, JsonSerializerOptions) ?? new Dictionary<string, JsonNode>());
        
        modelBuilder.Entity<EntryEntity>()
            .Property(e => e.Categories)
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonSerializerOptions),
                v => JsonSerializer.Deserialize<List<EntryCategoryEntity>>(v, JsonSerializerOptions) ?? new List<EntryCategoryEntity>());

        modelBuilder.Entity<ProfileContainerEntity>()
            .Property(e => e.ProfileConfiguration)
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonSerializerOptions),
                v => JsonSerializer.Deserialize<ProfileConfigurationEntity>(v, JsonSerializerOptions) ?? new ProfileConfigurationEntity());

        modelBuilder.Entity<ProfileContainerEntity>()
            .Property(e => e.Profile)
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonSerializerOptions),
                v => JsonSerializer.Deserialize<ProfileEntity>(v, JsonSerializerOptions) ?? new ProfileEntity());
    }
}
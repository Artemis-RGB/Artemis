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
    public DbSet<PluginSettingEntity> PluginSettings => Set<PluginSettingEntity>();
    public DbSet<ProfileCategoryEntity> ProfileCategories => Set<ProfileCategoryEntity>();
    public DbSet<ProfileContainerEntity> Profiles => Set<ProfileContainerEntity>();
    public DbSet<ReleaseEntity> Releases => Set<ReleaseEntity>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DeviceEntity>()
            .OwnsOne(d => d.InputIdentifiers, builder => builder.ToJson())
            .OwnsOne(d => d.InputMappings, builder => builder.ToJson());
            
        modelBuilder.Entity<EntryEntity>()
            .OwnsOne(e => e.Metadata, builder => builder.ToJson());

        modelBuilder.Entity<PluginSettingEntity>()
            .OwnsOne(s => s.Value, builder => builder.ToJson());

        modelBuilder.Entity<ProfileContainerEntity>()
            .OwnsOne(c => c.ProfileConfiguration, builder => builder.ToJson())
            .OwnsOne(c => c.Profile, builder => builder.ToJson());
    }
}
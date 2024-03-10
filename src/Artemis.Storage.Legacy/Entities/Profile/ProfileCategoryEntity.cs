using Artemis.Core;
using Artemis.Storage.Entities.Profile;
using LiteDB;
using Serilog;

namespace Artemis.Storage.Legacy.Entities.Profile;

internal class ProfileCategoryEntity
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public bool IsCollapsed { get; set; }
    public bool IsSuspended { get; set; }
    public int Order { get; set; }

    public List<ProfileConfigurationEntity> ProfileConfigurations { get; set; } = new();

    public Storage.Entities.Profile.ProfileCategoryEntity Migrate(ILogger logger, List<ProfileEntity> legacyProfiles, ILiteStorage<Guid> profileIcons)
    {
        Storage.Entities.Profile.ProfileCategoryEntity category = new()
        {
            Id = Id,
            Name = Name,
            IsCollapsed = IsCollapsed,
            IsSuspended = IsSuspended,
            Order = Order
        };

        foreach (ProfileConfigurationEntity legacyProfileConfiguration in ProfileConfigurations)
        {
            // Find the profile
            ProfileEntity? legacyProfile = legacyProfiles.FirstOrDefault(p => p.Id == legacyProfileConfiguration.ProfileId);
            if (legacyProfile == null)
            {
                logger.Information("Profile not found for profile configuration {ProfileId}", legacyProfileConfiguration.ProfileId);
                continue;
            }

            // Clone to the new format via JSON, as both are serializable
            string profileJson = CoreJson.Serialize(legacyProfile);
            string configJson = CoreJson.Serialize(legacyProfileConfiguration);
            Storage.Entities.Profile.ProfileEntity? profile = CoreJson.Deserialize<Storage.Entities.Profile.ProfileEntity>(profileJson);
            Storage.Entities.Profile.ProfileConfigurationEntity? config = CoreJson.Deserialize<Storage.Entities.Profile.ProfileConfigurationEntity>(configJson);

            if (profile == null)
            {
                logger.Information("Failed to deserialize profile JSON for profile configuration {ProfileId}", legacyProfileConfiguration.ProfileId);
                continue;
            }

            if (config == null)
            {
                logger.Information("Failed to deserialize profile configuration JSON for profile configuration {ProfileId}", legacyProfileConfiguration.ProfileId);
                continue;
            }

            // Add a container
            ProfileContainerEntity container = new()
            {
                Profile = profile,
                ProfileConfiguration = config,
            };

            // If available, download the profile icon
            if (profileIcons.Exists(legacyProfileConfiguration.FileIconId))
            {
                using MemoryStream memoryStream = new();
                profileIcons.Download(legacyProfileConfiguration.FileIconId, memoryStream);
                container.Icon = memoryStream.ToArray();
            }

            category.ProfileConfigurations.Add(container);
        }

        return category;
    }
}
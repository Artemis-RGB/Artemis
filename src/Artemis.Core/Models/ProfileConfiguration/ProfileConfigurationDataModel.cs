using Artemis.Core.Modules;

namespace Artemis.Core;

/// <summary>
///     The data model containing the configured values of all configurable profiles, each profile is a dynamic child
///     keyed by its profile ID.
/// </summary>
internal class ProfileConfigurationsDataModel : DataModel
{
    private ProfileConfigurationsDataModel()
    {
        Module = Constants.CorePluginFeature;
        DataModelDescription = new DataModelPropertyAttribute
        {
            Name = "Profile configuration",
            Description = "The values users configured on configurable profiles"
        };
        IsExpansion = true;
    }

    public static ProfileConfigurationsDataModel Instance { get; } = new();
}

/// <summary>
///     The data model containing the configured values of a single profile, each value is a dynamic child keyed by its
///     configuration item ID.
/// </summary>
internal class ProfileConfigurationDataModel : DataModel;
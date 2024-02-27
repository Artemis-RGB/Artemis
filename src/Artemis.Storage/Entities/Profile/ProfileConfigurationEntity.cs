using System;
using Artemis.Storage.Entities.Profile.Nodes;
using Serilog.Core;

namespace Artemis.Storage.Entities.Profile;

public class ProfileConfigurationEntity
{
    public string Name { get; set; } = string.Empty;
    public string? MaterialIcon { get; set; }
    public Guid FileIconId { get; set; }
    public int IconType { get; set; }
    public bool IconFill { get; set; }
    public int Order { get; set; }

    public bool IsSuspended { get; set; }
    public int ActivationBehaviour { get; set; }
    public NodeScriptEntity? ActivationCondition { get; set; }

    public int HotkeyMode { get; set; }
    public ProfileConfigurationHotkeyEntity? EnableHotkey { get; set; }
    public ProfileConfigurationHotkeyEntity? DisableHotkey { get; set; }

    public string? ModuleId { get; set; }

    public Guid ProfileCategoryId { get; set; }
    public Guid ProfileId { get; set; }

    public bool FadeInAndOut { get; set; }
    public int Version { get; set; } = StorageMigrationService.PROFILE_VERSION;
}
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Artemis.Core.Modules;
using Artemis.Storage.Entities.Profile;

namespace Artemis.Core;

/// <summary>
///     Represents the configuration of a profile, contained in a <see cref="ProfileCategory" />
/// </summary>
public class ProfileConfiguration : BreakableModel, IStorageModel, IDisposable, IPluginFeatureDependent
{
    /// <summary>
    /// Represents an empty profile.
    /// </summary>
    public static readonly ProfileConfiguration Empty = new(ProfileCategory.Empty, "Empty", "Empty");

    private bool _disposed;

    internal ProfileConfiguration(ProfileCategory category, string name, string icon)
    {
        Name = name;
        Category = category;

        Entity = new ProfileContainerEntity();
        Icon = new ProfileConfigurationIcon(Entity);
        Icon.SetIconByName(icon);
        ActivationCondition = new NodeScript<bool>("Activate profile", "Whether or not the profile should be active", this);

        Entity.Profile.Id = Guid.NewGuid();
        Entity.ProfileConfiguration.ProfileId = Entity.Profile.Id;
    }

    internal ProfileConfiguration(ProfileCategory category, ProfileContainerEntity entity)
    {
        // Will be loaded from the entity
        Name = null!;
        Category = category;

        Entity = entity;
        Icon = new ProfileConfigurationIcon(Entity);
        ActivationCondition = new NodeScript<bool>("Activate profile", "Whether or not the profile should be active", this);

        Load();
    }

    /// <summary>
    ///     Gets or sets the name of this profile configuration
    /// </summary>
    public string Name
    {
        get;
        set => SetAndNotify(ref field, value);
    }

    /// <summary>
    ///     The order in which this profile appears in the update loop and sidebar
    /// </summary>
    public int Order
    {
        get;
        set => SetAndNotify(ref field, value);
    }

    /// <summary>
    ///     Gets or sets a boolean indicating whether this profile is suspended, disabling it regardless of the
    ///     <see cref="ActivationCondition" />
    /// </summary>
    public bool IsSuspended
    {
        get;
        set => SetAndNotify(ref field, value);
    }

    /// <summary>
    ///     Gets a boolean indicating whether this profile configuration is missing any modules
    /// </summary>
    public bool IsMissingModule
    {
        get;
        private set => SetAndNotify(ref field, value);
    }

    /// <summary>
    ///     Gets or sets the category of this profile configuration
    /// </summary>
    public ProfileCategory Category
    {
        get;
        internal set => SetAndNotify(ref field, value);
    }

    /// <summary>
    ///     Gets or sets the <see cref="ProfileConfigurationHotkeyMode" /> used to determine hotkey behaviour
    /// </summary>
    public ProfileConfigurationHotkeyMode HotkeyMode
    {
        get;
        set => SetAndNotify(ref field, value);
    }

    /// <summary>
    ///     Gets or sets the hotkey used to enable or toggle the profile
    /// </summary>
    public Hotkey? EnableHotkey
    {
        get;
        set => SetAndNotify(ref field, value);
    }

    /// <summary>
    ///     Gets or sets the hotkey used to disable the profile
    /// </summary>
    public Hotkey? DisableHotkey
    {
        get;
        set => SetAndNotify(ref field, value);
    }

    /// <summary>
    ///     Gets or sets the behaviour of when this profile is activated
    /// </summary>
    public ActivationBehaviour ActivationBehaviour
    {
        get;
        set => SetAndNotify(ref field, value);
    }

    /// <summary>
    ///     Gets a boolean indicating whether the activation conditions where met during the last <see cref="Update" /> call
    /// </summary>
    public bool ActivationConditionMet
    {
        get;
        private set => SetAndNotify(ref field, value);
    }

    /// <summary>
    ///     Gets the profile of this profile configuration
    /// </summary>
    public Profile? Profile
    {
        get;
        internal set => SetAndNotify(ref field, value);
    }

    /// <summary>
    ///    Gets or sets a boolean indicating whether this profile should fade in and out when enabling or disabling
    /// </summary>
    public bool FadeInAndOut
    {
        get;
        set => SetAndNotify(ref field, value);
    }

    /// <summary>
    ///     Gets or sets a boolean indicating whether this profile is configurable via its <see cref="ConfigurationSections"/>.
    /// </summary>
    public bool IsConfigurable
    {
        get;
        set => SetAndNotify(ref field, value);
    }

    /// <summary>
    ///     Gets or sets the module this profile uses
    /// </summary>
    public Module? Module
    {
        get;
        set
        {
            SetAndNotify(ref field, value);
            IsMissingModule = false;
        }
    }

    /// <summary>
    ///     Gets the configuration sections of this profile configuration.
    /// </summary>
    public ObservableCollection<ConfigurationSection> ConfigurationSections { get; } = [];

    /// <summary>
    ///     Gets the icon configuration
    /// </summary>
    public ProfileConfigurationIcon Icon { get; }

    /// <summary>
    ///     Gets the data model condition that must evaluate to <see langword="true" /> for this profile to be activated
    ///     alongside any activation requirements of the <see cref="Module" />, if set
    /// </summary>
    public NodeScript<bool> ActivationCondition { get; }

    /// <summary>
    ///     Gets the entity used by this profile config
    /// </summary>
    public ProfileContainerEntity Entity { get; }

    /// <summary>
    ///     Gets the ID of the profile of this profile configuration
    /// </summary>
    public Guid ProfileId => Entity.Profile.Id;

    #region Overrides of BreakableModel

    /// <inheritdoc />
    public override string BrokenDisplayName => "Profile Configuration";

    #endregion

    /// <summary>
    ///     Updates this configurations activation condition status
    /// </summary>
    public void Update()
    {
        if (_disposed)
            throw new ObjectDisposedException("ProfileConfiguration");

        if (!ActivationCondition.ExitNodeConnected)
        {
            ActivationConditionMet = true;
        }
        else
        {
            ActivationCondition.Run();
            ActivationConditionMet = ActivationCondition.Result;
        }
    }

    /// <summary>
    ///     Determines whether the profile of this configuration should be active
    /// </summary>
    /// <param name="includeActivationCondition">Whether or not to take activation conditions into consideration</param>
    public bool ShouldBeActive(bool includeActivationCondition)
    {
        if (_disposed)
            throw new ObjectDisposedException("ProfileConfiguration");
        if (Category.IsSuspended || IsSuspended || IsMissingModule)
            return false;

        if (includeActivationCondition)
            return ActivationConditionMet && (Module == null || Module.IsActivated);
        return Module == null || Module.IsActivated;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"[ProfileConfiguration] {nameof(Name)}: {Name}";
    }

    /// <inheritdoc />
    public IEnumerable<PluginFeature> GetFeatureDependencies()
    {
        if (_disposed)
            throw new ObjectDisposedException("ProfileConfiguration");
        if (Profile == null)
            throw new InvalidOperationException("Cannot determine feature dependencies when the profile is not loaded.");

        return ActivationCondition.GetFeatureDependencies()
            .Concat(Profile.GetFeatureDependencies())
            .Concat(Module != null ? [Module] : []);
    }

    internal void LoadModules(List<Module> enabledModules)
    {
        if (_disposed)
            throw new ObjectDisposedException("ProfileConfiguration");

        Module = enabledModules.FirstOrDefault(m => m.Id == Entity.ProfileConfiguration.ModuleId);
        IsMissingModule = Module == null && Entity.ProfileConfiguration.ModuleId != null;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _disposed = true;
        ActivationCondition.Dispose();
    }

    #region Implementation of IStorageModel

    /// <inheritdoc />
    public void Load()
    {
        if (_disposed)
            throw new ObjectDisposedException("ProfileConfiguration");

        Name = Entity.ProfileConfiguration.Name;
        IsSuspended = Entity.ProfileConfiguration.IsSuspended;
        ActivationBehaviour = (ActivationBehaviour) Entity.ProfileConfiguration.ActivationBehaviour;
        HotkeyMode = (ProfileConfigurationHotkeyMode) Entity.ProfileConfiguration.HotkeyMode;
        FadeInAndOut = Entity.ProfileConfiguration.FadeInAndOut;
        Order = Entity.ProfileConfiguration.Order;

        Icon.Load();

        if (Entity.ProfileConfiguration.ActivationCondition != null)
            ActivationCondition.LoadFromEntity(Entity.ProfileConfiguration.ActivationCondition);

        EnableHotkey = Entity.ProfileConfiguration.EnableHotkey != null ? new Hotkey(Entity.ProfileConfiguration.EnableHotkey) : null;
        DisableHotkey = Entity.ProfileConfiguration.DisableHotkey != null ? new Hotkey(Entity.ProfileConfiguration.DisableHotkey) : null;

        // Placeholder configuration sections
        ConfigurationSections.Clear();
        ConfigurationSections.Add(new ConfigurationSection()
        {
            Name = "General",
        });
        ConfigurationSections.Add(new ConfigurationSection()
        {
            Name = "Other",
        });
        ConfigurationSections.Add(new ConfigurationSection()
        {
            Name = "Something else",
        });
        ConfigurationSections[0].Items.Add(new ConfigurationTextItem() {Text = "This is a placeholder text item in the General section."});
        ConfigurationSections[0].Items.Add(new ConfigurationNumericItem() {Name = "Numeric item", Description = "This one also has a description"});
        ConfigurationSections[0].Items.Add(new ConfigurationBooleanItem() {Name = "Do the thing?", TrueText = "Absolutely", FalseText = "Nope"});
        ConfigurationSections[1].Items.Add(new ConfigurationTextItem() {Text = "This is a placeholder text item in the Other section."});
        ConfigurationSections[2].Items.Add(new ConfigurationTextItem() {Text = "This is a placeholder text item in the Something else section."});
        ConfigurationSections[2].Items.Add(new ConfigurationTextItem() {Text = "This is another placeholder text item in the Something else section."});
        ConfigurationSections[2].Items.Add(new ConfigurationNumericItem() {Name = "Numeric item", Description = "This one uses a slider", Minimum = 0, Maximum = 10, Slider = true});
        ConfigurationSections[2].Items.Add(new ConfigurationColorGradientItem() {Name = "Color gradient"});
        ConfigurationSections[2].Items.Add(new ConfigurationSKColorItem() {Name = "A simple color", Description = "Again with a description"});
    }

    /// <inheritdoc />
    public void Save()
    {
        if (_disposed)
            throw new ObjectDisposedException("ProfileConfiguration");

        Entity.ProfileConfiguration.Name = Name;
        Entity.ProfileConfiguration.IsSuspended = IsSuspended;
        Entity.ProfileConfiguration.ActivationBehaviour = (int) ActivationBehaviour;
        Entity.ProfileConfiguration.HotkeyMode = (int) HotkeyMode;
        Entity.ProfileConfiguration.ProfileCategoryId = Category.Entity.Id;
        Entity.ProfileConfiguration.FadeInAndOut = FadeInAndOut;
        Entity.ProfileConfiguration.Order = Order;

        Icon.Save();

        ActivationCondition.Save();
        Entity.ProfileConfiguration.ActivationCondition = ActivationCondition.Entity;

        EnableHotkey?.Save();
        Entity.ProfileConfiguration.EnableHotkey = EnableHotkey?.Entity;
        DisableHotkey?.Save();
        Entity.ProfileConfiguration.DisableHotkey = DisableHotkey?.Entity;

        if (!IsMissingModule)
            Entity.ProfileConfiguration.ModuleId = Module?.Id;
    }

    #endregion
}

/// <summary>
///     Represents a type of behaviour when this profile is activated
/// </summary>
public enum ActivationBehaviour
{
    /// <summary>
    ///     Do nothing to other profiles
    /// </summary>
    None,

    /// <summary>
    ///     Disable all other profiles
    /// </summary>
    DisableOthers,

    /// <summary>
    ///     Disable all other profiles below this one
    /// </summary>
    DisableOthersBelow,

    /// <summary>
    ///     Disable all other profiles above this one
    /// </summary>
    DisableOthersAbove,

    /// <summary>
    ///     Disable all other profiles in the same category
    /// </summary>
    DisableOthersInCategory,

    /// <summary>
    ///     Disable all other profiles below this one in the same category
    /// </summary>
    DisableOthersBelowInCategory,

    /// <summary>
    ///     Disable all other profiles above this one in the same category
    /// </summary>
    DisableOthersAboveInCategory
}

/// <summary>
///     Represents a hotkey mode for a profile configuration
/// </summary>
public enum ProfileConfigurationHotkeyMode
{
    /// <summary>
    ///     Use no hotkeys
    /// </summary>
    None,

    /// <summary>
    ///     Toggle the profile with one hotkey
    /// </summary>
    Toggle,

    /// <summary>
    ///     Enable and disable the profile with two separate hotkeys
    /// </summary>
    EnableDisable
}
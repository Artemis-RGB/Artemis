using System;
using System.Collections.Generic;
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

    private ActivationBehaviour _activationBehaviour;
    private bool _activationConditionMet;
    private ProfileCategory _category;
    private Hotkey? _disableHotkey;
    private bool _disposed;
    private Hotkey? _enableHotkey;
    private ProfileConfigurationHotkeyMode _hotkeyMode;
    private bool _isMissingModule;
    private bool _isSuspended;
    private bool _fadeInAndOut;
    private Module? _module;

    private string _name;
    private int _order;
    private Profile? _profile;

    internal ProfileConfiguration(ProfileCategory category, string name, string icon)
    {
        _name = name;
        _category = category;

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
        _name = null!;
        _category = category;

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
        get => _name;
        set => SetAndNotify(ref _name, value);
    }

    /// <summary>
    ///     The order in which this profile appears in the update loop and sidebar
    /// </summary>
    public int Order
    {
        get => _order;
        set => SetAndNotify(ref _order, value);
    }

    /// <summary>
    ///     Gets or sets a boolean indicating whether this profile is suspended, disabling it regardless of the
    ///     <see cref="ActivationCondition" />
    /// </summary>
    public bool IsSuspended
    {
        get => _isSuspended;
        set => SetAndNotify(ref _isSuspended, value);
    }

    /// <summary>
    ///     Gets a boolean indicating whether this profile configuration is missing any modules
    /// </summary>
    public bool IsMissingModule
    {
        get => _isMissingModule;
        private set => SetAndNotify(ref _isMissingModule, value);
    }

    /// <summary>
    ///     Gets or sets the category of this profile configuration
    /// </summary>
    public ProfileCategory Category
    {
        get => _category;
        internal set => SetAndNotify(ref _category, value);
    }

    /// <summary>
    ///     Gets or sets the <see cref="ProfileConfigurationHotkeyMode" /> used to determine hotkey behaviour
    /// </summary>
    public ProfileConfigurationHotkeyMode HotkeyMode
    {
        get => _hotkeyMode;
        set => SetAndNotify(ref _hotkeyMode, value);
    }

    /// <summary>
    ///     Gets or sets the hotkey used to enable or toggle the profile
    /// </summary>
    public Hotkey? EnableHotkey
    {
        get => _enableHotkey;
        set => SetAndNotify(ref _enableHotkey, value);
    }

    /// <summary>
    ///     Gets or sets the hotkey used to disable the profile
    /// </summary>
    public Hotkey? DisableHotkey
    {
        get => _disableHotkey;
        set => SetAndNotify(ref _disableHotkey, value);
    }

    /// <summary>
    ///     Gets or sets the behaviour of when this profile is activated
    /// </summary>
    public ActivationBehaviour ActivationBehaviour
    {
        get => _activationBehaviour;
        set => SetAndNotify(ref _activationBehaviour, value);
    }

    /// <summary>
    ///     Gets a boolean indicating whether the activation conditions where met during the last <see cref="Update" /> call
    /// </summary>
    public bool ActivationConditionMet
    {
        get => _activationConditionMet;
        private set => SetAndNotify(ref _activationConditionMet, value);
    }

    /// <summary>
    ///     Gets the profile of this profile configuration
    /// </summary>
    public Profile? Profile
    {
        get => _profile;
        internal set => SetAndNotify(ref _profile, value);
    }

    /// <summary>
    ///    Gets or sets a boolean indicating whether this profile should fade in and out when enabling or disabling
    /// </summary>
    public bool FadeInAndOut
    {
        get => _fadeInAndOut;
        set => SetAndNotify(ref _fadeInAndOut, value);
    }

    /// <summary>
    ///     Gets or sets the module this profile uses
    /// </summary>
    public Module? Module
    {
        get => _module;
        set
        {
            SetAndNotify(ref _module, value);
            IsMissingModule = false;
        }
    }

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
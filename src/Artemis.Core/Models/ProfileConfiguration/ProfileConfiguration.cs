using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Artemis.Core.Modules;
using Artemis.Storage.Entities.Profile;
using Artemis.Storage.Entities.Profile.Configuration;

namespace Artemis.Core;

/// <summary>
///     Represents the configuration of a profile, contained in a <see cref="ProfileCategory" />
/// </summary>
public class ProfileConfiguration : BreakableModel, IStorageModel, IDisposable, IPluginFeatureDependent
{
    /// <summary>
    ///     Represents an empty profile.
    /// </summary>
    public static readonly ProfileConfiguration Empty = new(ProfileCategory.Empty, "Empty", "Empty");

    private readonly ProfileConfigurationDataModel _configurationDataModel = new();
    private readonly HashSet<ConfigurationSection> _trackedSections = [];
    private readonly HashSet<IConfigurationItem> _trackedItems = [];
    private bool _disposed;
    private bool _configurationDataModelRegistered;

    internal ProfileConfiguration(ProfileCategory category, string name, string icon)
    {
        Name = name;
        Category = category;

        Entity = new ProfileContainerEntity();
        Icon = new ProfileConfigurationIcon(Entity);
        Icon.SetIconByName(icon);
        ActivationCondition = new NodeScript<bool>("Activate profile", "Whether or not the profile should be active", this);
        ConfigurationSections.CollectionChanged += ConfigurationSectionsOnCollectionChanged;

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
        ConfigurationSections.CollectionChanged += ConfigurationSectionsOnCollectionChanged;

        Load();
    }

    /// <summary>
    ///     Gets or sets the name of this profile configuration
    /// </summary>
    public string Name
    {
        get;
        set
        {
            if (SetAndNotify(ref field, value))
                UpdateConfigurationDataModel();
        }
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
    ///     Gets or sets a boolean indicating whether this profile should fade in and out when enabling or disabling
    /// </summary>
    public bool FadeInAndOut
    {
        get;
        set => SetAndNotify(ref field, value);
    }

    /// <summary>
    ///     Gets or sets a boolean indicating whether this profile is configurable via its <see cref="ConfigurationSections" />.
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
    ///     Gets all configuration items in all <see cref="ConfigurationSections" /> that accept user input.
    /// </summary>
    public IEnumerable<ConfigurationInputItem> ConfigurationInputItems => ConfigurationSections.SelectMany(s => s.Items).OfType<ConfigurationInputItem>();

    /// <summary>
    ///     Occurs when anything in the <see cref="ConfigurationSections" /> changed, including the values of items.
    /// </summary>
    public event EventHandler? ConfigurationChanged;

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

    /// <summary>
    ///     Resets the value of every configuration item to its default value.
    /// </summary>
    public void ResetConfigurationToDefaults()
    {
        foreach (ConfigurationInputItem item in ConfigurationInputItems)
            item.ResetToDefault();
    }

    /// <summary>
    ///     Copies the configured values of the provided profile configuration onto the matching configuration items of this
    ///     profile configuration. Items are matched by their ID, items without a match keep their current value.
    /// </summary>
    /// <param name="source">The profile configuration to copy the values from.</param>
    public void CopyConfigurationValues(ProfileConfiguration source)
    {
        Dictionary<Guid, ConfigurationInputItem> sourceItems = source.ConfigurationInputItems.ToDictionary(i => i.Id);
        foreach (ConfigurationInputItem item in ConfigurationInputItems)
        {
            if (sourceItems.TryGetValue(item.Id, out ConfigurationInputItem? sourceItem) && sourceItem.GetType() == item.GetType())
                item.DeserializeValue(sourceItem.SerializeValue());
        }
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"[ProfileConfiguration] {nameof(Name)}: {Name}";
    }

    /// <summary>
    ///     Invokes the <see cref="ConfigurationChanged" /> event
    /// </summary>
    protected virtual void OnConfigurationChanged()
    {
        ConfigurationChanged?.Invoke(this, EventArgs.Empty);
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
        RemoveConfigurationDataModel();
        _disposed = true;
        ActivationCondition.Dispose();
    }

    #region Configuration

    /// <summary>
    ///     Exposes the configuration values of this profile in the data model, done once the profile is part of the profile service.
    /// </summary>
    internal void AddConfigurationDataModel()
    {
        _configurationDataModelRegistered = true;
        UpdateConfigurationDataModel();
    }

    /// <summary>
    ///     Removes the configuration values of this profile from the data model, done once the profile is removed from the profile service.
    /// </summary>
    internal void RemoveConfigurationDataModel()
    {
        _configurationDataModelRegistered = false;
        UpdateConfigurationDataModel();
    }

    private void UpdateConfigurationDataModel()
    {
        if (_disposed || Entity == null)
            return;

        List<ConfigurationInputItem> items = ConfigurationInputItems.ToList();

        // Sync the values of this profile
        HashSet<string> keys = items.Select(i => i.Id.ToString("N")).ToHashSet();
        foreach (string key in _configurationDataModel.DynamicChildren.Keys.Where(k => !keys.Contains(k)).ToList())
            _configurationDataModel.RemoveDynamicChildByKey(key);
        foreach (ConfigurationInputItem item in items)
        {
            string key = item.Id.ToString("N");
            if (_configurationDataModel.TryGetDynamicChild(key, out DynamicChild? dynamicChild))
            {
                dynamicChild.Attribute.Name = item.Name;
                dynamicChild.Attribute.Description = item.Description;
                item.UpdateDynamicChild(_configurationDataModel, dynamicChild);
            }
            else
            {
                item.AddDynamicChild(_configurationDataModel, key, new DataModelPropertyAttribute {Name = item.Name, Description = item.Description});
            }
        }

        // Only list this profile in the data model if there's actually something to show
        ProfileConfigurationsDataModel root = ProfileConfigurationsDataModel.Instance;
        string profileKey = ProfileId.ToString("N");
        bool shouldBeInRoot = _configurationDataModelRegistered && items.Count > 0;
        bool isInRoot = root.TryGetDynamicChild(profileKey, out DynamicChild? profileChild) && profileChild.BaseValue == _configurationDataModel;

        if (shouldBeInRoot && !isInRoot)
        {
            root.RemoveDynamicChildByKey(profileKey);
            root.AddDynamicChild(profileKey, _configurationDataModel, Name, "The configuration values of this profile");
        }
        else if (!shouldBeInRoot && isInRoot)
        {
            root.RemoveDynamicChildByKey(profileKey);
        }
        else if (isInRoot)
        {
            profileChild!.Attribute.Name = Name;
        }
    }

    private void TrackConfiguration()
    {
        foreach (ConfigurationSection section in _trackedSections.Except(ConfigurationSections).ToList())
        {
            section.PropertyChanged -= ConfigurationOnPropertyChanged;
            section.Items.CollectionChanged -= ConfigurationOnCollectionChanged;
            _trackedSections.Remove(section);
        }

        foreach (ConfigurationSection section in ConfigurationSections.Except(_trackedSections).ToList())
        {
            section.PropertyChanged += ConfigurationOnPropertyChanged;
            section.Items.CollectionChanged += ConfigurationOnCollectionChanged;
            _trackedSections.Add(section);
        }

        List<IConfigurationItem> items = ConfigurationSections.SelectMany(s => s.Items).ToList();
        foreach (IConfigurationItem item in _trackedItems.Except(items).ToList())
        {
            item.PropertyChanged -= ConfigurationOnPropertyChanged;
            _trackedItems.Remove(item);
        }

        foreach (IConfigurationItem item in items.Except(_trackedItems).ToList())
        {
            item.PropertyChanged += ConfigurationOnPropertyChanged;
            _trackedItems.Add(item);
        }
    }

    private void ConfigurationSectionsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        ConfigurationOnCollectionChanged(sender, e);
    }

    private void ConfigurationOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        TrackConfiguration();
        UpdateConfigurationDataModel();
        OnConfigurationChanged();
    }

    private void ConfigurationOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        UpdateConfigurationDataModel();
        OnConfigurationChanged();
    }

    #endregion

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

        IsConfigurable = Entity.ProfileConfiguration.IsConfigurable;
        ConfigurationSections.Clear();
        foreach (ConfigurationSectionEntity sectionEntity in Entity.ProfileConfiguration.ConfigurationSections)
            ConfigurationSections.Add(ConfigurationSection.Load(this, sectionEntity));

        // Apply the values picked by the user, falling back to the defaults set by the author
        foreach (ConfigurationInputItem item in ConfigurationInputItems)
        {
            if (Entity.ProfileConfiguration.ConfigurationValues.TryGetValue(item.Id, out string? value))
                item.DeserializeValue(value);
            else
                item.ResetToDefault();
        }
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

        Entity.ProfileConfiguration.IsConfigurable = IsConfigurable;
        Entity.ProfileConfiguration.ConfigurationSections = ConfigurationSections.Select(s => s.Save()).ToList();
        Entity.ProfileConfiguration.ConfigurationValues = [];
        foreach (ConfigurationInputItem item in ConfigurationInputItems)
        {
            string? value = item.SerializeValue();
            if (value != null)
                Entity.ProfileConfiguration.ConfigurationValues[item.Id] = value;
        }
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
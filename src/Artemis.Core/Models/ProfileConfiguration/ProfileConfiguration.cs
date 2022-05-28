using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.Core.Modules;
using Artemis.Storage.Entities.Profile;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents the configuration of a profile, contained in a <see cref="ProfileCategory" />
    /// </summary>
    public class ProfileConfiguration : BreakableModel, IStorageModel, IDisposable
    {
        private ProfileCategory _category;
        private bool _disposed;

        private bool _isMissingModule;
        private bool _isSuspended;
        private Module? _module;
        private string _name;
        private int _order;
        private Profile? _profile;

        internal ProfileConfiguration(ProfileCategory category, string name, string icon)
        {
            _name = name;
            _category = category;

            Entity = new ProfileConfigurationEntity();
            Icon = new ProfileConfigurationIcon(Entity);
            Icon.SetIconByName(icon);
            ActivationCondition = new NodeScript<bool>("Activate profile", "Whether or not the profile should be active", this);
        }

        internal ProfileConfiguration(ProfileCategory category, ProfileConfigurationEntity entity)
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
        ///     Gets the icon configuration
        /// </summary>
        public ProfileConfigurationIcon Icon { get; }

        /// <summary>
        ///     Gets or sets the <see cref="ProfileConfigurationHotkeyMode" /> used to determine hotkey behaviour
        /// </summary>
        public ProfileConfigurationHotkeyMode HotkeyMode { get; set; }

        /// <summary>
        ///     Gets or sets the hotkey used to enable or toggle the profile
        /// </summary>
        public Hotkey? EnableHotkey { get; set; }

        /// <summary>
        ///     Gets or sets the hotkey used to disable the profile
        /// </summary>
        public Hotkey? DisableHotkey { get; set; }

        /// <summary>
        ///     Gets the ID of the profile of this profile configuration
        /// </summary>
        public Guid ProfileId => Entity.ProfileId;

        /// <summary>
        ///     Gets the profile of this profile configuration
        /// </summary>
        public Profile? Profile
        {
            get => _profile;
            internal set => SetAndNotify(ref _profile, value);
        }

        /// <summary>
        ///     Gets or sets the behaviour of when this profile is activated
        /// </summary>
        public ActivationBehaviour ActivationBehaviour { get; set; }

        /// <summary>
        ///     Gets the data model condition that must evaluate to <see langword="true" /> for this profile to be activated
        ///     alongside any activation requirements of the <see cref="Module" />, if set
        /// </summary>
        public NodeScript<bool> ActivationCondition { get; }

        /// <summary>
        ///     Gets or sets the module this profile uses
        /// </summary>
        public Module? Module
        {
            get => _module;
            set
            {
                _module = value;
                IsMissingModule = false;
            }
        }

        /// <summary>
        ///     Gets a boolean indicating whether the activation conditions where met during the last <see cref="Update" /> call
        /// </summary>
        public bool ActivationConditionMet { get; private set; }

        /// <summary>
        ///     Gets or sets a boolean indicating whether this profile configuration is being edited
        /// </summary>
        public bool IsBeingEdited { get; set; }

        /// <summary>
        ///     Gets the entity used by this profile config
        /// </summary>
        public ProfileConfigurationEntity Entity { get; }

        /// <summary>
        ///     Updates this configurations activation condition status
        /// </summary>
        public void Update()
        {
            if (_disposed)
                throw new ObjectDisposedException("ProfileConfiguration");

            if (!ActivationCondition.ExitNodeConnected)
                ActivationConditionMet = true;
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
            if (IsBeingEdited)
                return true;
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

        internal void LoadModules(List<Module> enabledModules)
        {
            if (_disposed)
                throw new ObjectDisposedException("ProfileConfiguration");

            Module = enabledModules.FirstOrDefault(m => m.Id == Entity.ModuleId);
            IsMissingModule = Module == null && Entity.ModuleId != null;
        }

        #region IDisposable

        /// <inheritdoc />
        public void Dispose()
        {
            _disposed = true;
            ActivationCondition.Dispose();
        }

        #endregion

        #region Implementation of IStorageModel

        /// <inheritdoc />
        public void Load()
        {
            if (_disposed)
                throw new ObjectDisposedException("ProfileConfiguration");

            Name = Entity.Name;
            IsSuspended = Entity.IsSuspended;
            ActivationBehaviour = (ActivationBehaviour) Entity.ActivationBehaviour;
            HotkeyMode = (ProfileConfigurationHotkeyMode) Entity.HotkeyMode;
            Order = Entity.Order;

            Icon.Load();

            ActivationCondition.LoadFromEntity(Entity.ActivationCondition);
            EnableHotkey = Entity.EnableHotkey != null ? new Hotkey(Entity.EnableHotkey) : null;
            DisableHotkey = Entity.DisableHotkey != null ? new Hotkey(Entity.DisableHotkey) : null;
        }

        /// <inheritdoc />
        public void Save()
        {
            if (_disposed)
                throw new ObjectDisposedException("ProfileConfiguration");

            Entity.Name = Name;
            Entity.IsSuspended = IsSuspended;
            Entity.ActivationBehaviour = (int) ActivationBehaviour;
            Entity.HotkeyMode = (int) HotkeyMode;
            Entity.ProfileCategoryId = Category.Entity.Id;
            Entity.Order = Order;

            Icon.Save();

            ActivationCondition.Save();
            Entity.ActivationCondition = ActivationCondition.Entity;

            EnableHotkey?.Save();
            Entity.EnableHotkey = EnableHotkey?.Entity;
            DisableHotkey?.Save();
            Entity.DisableHotkey = DisableHotkey?.Entity;

            if (!IsMissingModule)
                Entity.ModuleId = Module?.Id;
        }

        #endregion

        #region Overrides of BreakableModel

        /// <inheritdoc />
        public override string BrokenDisplayName => "Profile Configuration";

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
}
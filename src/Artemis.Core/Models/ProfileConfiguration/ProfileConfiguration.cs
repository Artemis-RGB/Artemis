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
    public class ProfileConfiguration : CorePropertyChanged, IStorageModel, IDisposable
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
            Icon = new ProfileConfigurationIcon(Entity) {MaterialIcon = icon};
        }

        internal ProfileConfiguration(ProfileCategory category, ProfileConfigurationEntity entity)
        {
            // Will be loaded from the entity
            _name = null!;
            _category = category;

            Entity = entity;
            Icon = new ProfileConfigurationIcon(Entity);
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
        public DataModelConditionGroup? ActivationCondition { get; set; }

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

            ActivationConditionMet = ActivationCondition == null || ActivationCondition.Evaluate();
        }

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
            ActivationCondition?.Dispose();
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
            Order = Entity.Order;

            Icon.Load();

            ActivationCondition = Entity.ActivationCondition != null
                ? new DataModelConditionGroup(null, Entity.ActivationCondition)
                : null;
        }

        /// <inheritdoc />
        public void Save()
        {
            if (_disposed)
                throw new ObjectDisposedException("ProfileConfiguration");

            Entity.Name = Name;
            Entity.IsSuspended = IsSuspended;
            Entity.ActivationBehaviour = (int) ActivationBehaviour;
            Entity.ProfileCategoryId = Category.Entity.Id;
            Entity.Order = Order;

            Icon.Save();

            if (ActivationCondition != null)
            {
                ActivationCondition.Save();
                Entity.ActivationCondition = ActivationCondition.Entity;
            }
            else
                Entity.ActivationCondition = null;

            if (!IsMissingModule)
                Entity.ModuleId = Module?.Id;
        }

        #endregion
    }
}
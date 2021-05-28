using System;
using System.Collections.Generic;
using Artemis.Storage.Entities.Profile;

namespace Artemis.Core
{
    public class ProfileCategory : CorePropertyChanged, IStorageModel
    {
        private readonly List<ProfileConfiguration> _profileConfigurations = new();
        private bool _isCollapsed;
        private bool _isSuspended;
        private string _name;

        /// <summary>
        ///     Creates a new instance of the <see cref="ProfileCategory" /> class
        /// </summary>
        /// <param name="name">The name of the category</param>
        internal ProfileCategory(string name)
        {
            _name = name;
            Entity = new ProfileCategoryEntity();
        }

        internal ProfileCategory(ProfileCategoryEntity entity)
        {
            Entity = entity;
            Load();
        }

        /// <summary>
        ///     Gets or sets the name of the profile category
        /// </summary>
        public string Name
        {
            get => _name;
            set => SetAndNotify(ref _name, value);
        }

        /// <summary>
        ///     Gets or sets a boolean indicating whether the category is collapsed or not
        ///     <para>Note: Has no implications other than inside the UI</para>
        /// </summary>
        public bool IsCollapsed
        {
            get => _isCollapsed;
            set => SetAndNotify(ref _isCollapsed, value);
        }

        /// <summary>
        ///     Gets or sets a boolean indicating whether this category is suspended, disabling all its profiles
        /// </summary>
        public bool IsSuspended
        {
            get => _isSuspended;
            set => SetAndNotify(ref _isSuspended, value);
        }

        /// <summary>
        ///     Gets a read only collection of the profiles inside this category
        /// </summary>
        public IReadOnlyCollection<ProfileConfiguration> ProfileConfigurations => _profileConfigurations.AsReadOnly();

        /// <summary>
        ///     Gets the unique ID of this category
        /// </summary>
        public Guid EntityId => Entity.Id;

        internal ProfileCategoryEntity Entity { get; }

        internal void AddProfileConfiguration(ProfileConfiguration configuration)
        {
            _profileConfigurations.Add(configuration);
            configuration.Category = this;
            OnProfileConfigurationAdded(new ProfileConfigurationEventArgs(configuration));
        }

        internal void RemoveProfileConfiguration(ProfileConfiguration configuration)
        {
            if (_profileConfigurations.Remove(configuration))
                OnProfileConfigurationRemoved(new ProfileConfigurationEventArgs(configuration));
        }

        #region Implementation of IStorageModel

        /// <inheritdoc />
        public void Load()
        {
            Name = Entity.Name;
            IsCollapsed = Entity.IsCollapsed;
            IsSuspended = Entity.IsSuspended;

            _profileConfigurations.Clear();
            foreach (ProfileConfigurationEntity entityProfileConfiguration in Entity.ProfileConfigurations)
                _profileConfigurations.Add(new ProfileConfiguration(this, entityProfileConfiguration));
        }

        /// <inheritdoc />
        public void Save()
        {
            Entity.Name = Name;
            Entity.IsCollapsed = IsCollapsed;
            Entity.IsSuspended = IsSuspended;

            Entity.ProfileConfigurations.Clear();
            foreach (ProfileConfiguration profileConfiguration in ProfileConfigurations)
            {
                profileConfiguration.Save();
                Entity.ProfileConfigurations.Add(profileConfiguration.Entity);
            }
        }

        #endregion

        #region Events

        /// <summary>
        ///     Occurs when a profile configuration is added to this <see cref="ProfileCategory" />
        /// </summary>
        public event EventHandler<ProfileConfigurationEventArgs>? ProfileConfigurationAdded;

        /// <summary>
        ///     Occurs when a profile configuration is removed from this <see cref="ProfileCategory" />
        /// </summary>
        public event EventHandler<ProfileConfigurationEventArgs>? ProfileConfigurationRemoved;

        /// <summary>
        ///     Invokes the <see cref="ProfileConfigurationAdded" /> event
        /// </summary>
        protected virtual void OnProfileConfigurationAdded(ProfileConfigurationEventArgs e)
        {
            ProfileConfigurationAdded?.Invoke(this, e);
        }

        /// <summary>
        ///     Invokes the <see cref="ProfileConfigurationRemoved" /> event
        /// </summary>
        protected virtual void OnProfileConfigurationRemoved(ProfileConfigurationEventArgs e)
        {
            ProfileConfigurationRemoved?.Invoke(this, e);
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
}
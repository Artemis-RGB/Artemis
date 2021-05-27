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

        internal ProfileCategoryEntity Entity { get; }

        /// <summary>
        ///     Creates a new profile configuration and adds it to the <see cref="ProfileCategory" />
        /// </summary>
        /// <param name="name">The name of the new profile configuration</param>
        /// <param name="icon">The icon of the new profile configuration</param>
        /// <returns>The newly created profile configuration</returns>
        public ProfileConfiguration AddProfileConfiguration(string name, string icon)
        {
            ProfileConfiguration configuration = new(name, icon, this);
            _profileConfigurations.Add(configuration);
            OnProfileConfigurationAdded(new ProfileConfigurationEventArgs(configuration));
            return configuration;
        }

        /// <summary>
        ///     Removes the provided profile configuration from the <see cref="ProfileCategory" />
        /// </summary>
        /// <param name="profileConfiguration"></param>
        public void RemoveProfileConfiguration(ProfileConfiguration profileConfiguration)
        {
            if (_profileConfigurations.Remove(profileConfiguration))
                OnProfileConfigurationRemoved(new ProfileConfigurationEventArgs(profileConfiguration));
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

    public enum ActivationBehaviour
    {
        None,
        DisableOthers,
        DisableOthersBelow,
        DisableOthersAbove,
        DisableOthersInCategory,
        DisableOthersBelowInCategory,
        DisableOthersAboveInCategory
    }
}
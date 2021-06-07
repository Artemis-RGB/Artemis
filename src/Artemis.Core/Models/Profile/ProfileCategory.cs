using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Artemis.Storage.Entities.Profile;

namespace Artemis.Core
{
    public class ProfileCategory : CorePropertyChanged, IStorageModel
    {
        private readonly List<ProfileConfiguration> _profileConfigurations = new();
        private bool _isCollapsed;
        private bool _isSuspended;
        private string _name;
        private int _order;

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
        ///     The order in which this category appears in the update loop and sidebar
        /// </summary>
        public int Order
        {
            get => _order;
            set => SetAndNotify(ref _order, value);
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
        public ReadOnlyCollection<ProfileConfiguration> ProfileConfigurations => _profileConfigurations.AsReadOnly();

        /// <summary>
        ///     Gets the unique ID of this category
        /// </summary>
        public Guid EntityId => Entity.Id;

        internal ProfileCategoryEntity Entity { get; }


        /// <summary>
        ///     Adds a profile configuration to this category
        /// </summary>
        public void AddProfileConfiguration(ProfileConfiguration configuration, int? targetIndex)
        {
            // Removing the original will shift every item in the list forwards, keep that in mind with the target index
            if (configuration.Category == this && targetIndex != null && targetIndex.Value > _profileConfigurations.IndexOf(configuration))
                targetIndex -= 1;

            configuration.Category.RemoveProfileConfiguration(configuration);

            if (targetIndex != null)
                _profileConfigurations.Insert(Math.Clamp(targetIndex.Value, 0, _profileConfigurations.Count), configuration);
            else
                _profileConfigurations.Add(configuration);
            configuration.Category = this;

            for (int index = 0; index < _profileConfigurations.Count; index++)
                _profileConfigurations[index].Order = index;
            OnProfileConfigurationAdded(new ProfileConfigurationEventArgs(configuration));
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"[ProfileCategory] {Order} {nameof(Name)}: {Name}, {nameof(IsSuspended)}: {IsSuspended}";
        }

        internal void RemoveProfileConfiguration(ProfileConfiguration configuration)
        {
            if (!_profileConfigurations.Remove(configuration)) return;

            for (int index = 0; index < _profileConfigurations.Count; index++)
                _profileConfigurations[index].Order = index;
            OnProfileConfigurationRemoved(new ProfileConfigurationEventArgs(configuration));
        }

        #region Implementation of IStorageModel

        /// <inheritdoc />
        public void Load()
        {
            Name = Entity.Name;
            IsCollapsed = Entity.IsCollapsed;
            IsSuspended = Entity.IsSuspended;
            Order = Entity.Order;

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
            Entity.Order = Order;

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
    ///     Represents a name of one of the default categories
    /// </summary>
    public enum DefaultCategoryName
    {
        /// <summary>
        ///     The category used by profiles tied to games
        /// </summary>
        Games,

        /// <summary>
        ///     The category used by profiles tied to applications
        /// </summary>
        Applications,

        /// <summary>
        ///     The category used by general profiles
        /// </summary>
        General
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
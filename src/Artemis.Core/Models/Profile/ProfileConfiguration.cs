using System.Collections.Generic;
using System.Linq;
using Artemis.Core.Modules;
using Artemis.Storage.Entities.Profile;

namespace Artemis.Core
{
    public class ProfileConfiguration : IStorageModel
    {
        private Module? _module;

        internal ProfileConfiguration(string name, string icon, ProfileCategory category)
        {
            Name = name;
            Icon = icon;
            Category = category;
            Entity = new ProfileConfigurationEntity();
        }

        internal ProfileConfiguration(ProfileCategory category, ProfileConfigurationEntity entity)
        {
            // Will be loaded from the entity
            Name = null!;
            Icon = null!;
            Category = category;
            Entity = entity;
            Load();
        }

        /// <summary>
        ///     Gets or sets the name of this profile configuration
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Gets or sets the icon of this profile configuration
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        ///     Gets or sets a boolean indicating whether this profile is suspended, disabling it regardless of the
        ///     <see cref="ActivationCondition" />
        /// </summary>
        public bool IsSuspended { get; set; }

        /// <summary>
        ///     Gets a boolean indicating whether this profile configuration is missing any modules
        /// </summary>
        public bool IsMissingModule { get; private set; }

        /// <summary>
        ///     Gets or sets the behaviour of when this profile is activated
        /// </summary>
        public ActivationBehaviour ActivationBehaviour { get; set; }

        /// <summary>
        ///     Gets or sets the category of this profile configuration
        /// </summary>
        public ProfileCategory Category { get; set; }

        /// <summary>
        ///     Gets the profile of this profile configuration
        /// </summary>
        public Profile? Profile { get; internal set; }

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

        internal ProfileConfigurationEntity Entity { get; }

        /// <summary>
        ///     Updates this configurations activation condition status
        /// </summary>
        public void Update()
        {
            ActivationConditionMet = ActivationCondition == null || ActivationCondition.Evaluate();
        }

        internal void LoadModules(List<Module> enabledModules)
        {
            Module = enabledModules.FirstOrDefault(m => m.Id == Entity.ModuleId);
            IsMissingModule = Module == null && Entity.ModuleId != null;
        }

        public bool ShouldBeActive(bool includeActivationCondition)
        {
            if (IsSuspended || IsMissingModule)
                return false;

            if (includeActivationCondition)
                return ActivationConditionMet && (Module == null || Module.IsActivated);
            return Module == null || Module.IsActivated;
        }

        #region Implementation of IStorageModel

        /// <inheritdoc />
        public void Load()
        {
            Name = Entity.Name;
            Icon = Entity.Icon;
            IsSuspended = Entity.IsSuspended;
            ActivationBehaviour = (ActivationBehaviour) Entity.ActivationBehaviour;

            ActivationCondition = Entity.ActivationCondition != null
                ? new DataModelConditionGroup(null, Entity.ActivationCondition)
                : null;
        }

        /// <inheritdoc />
        public void Save()
        {
            Entity.Name = Name;
            Entity.Icon = Icon;
            Entity.IsSuspended = IsSuspended;
            Entity.ActivationBehaviour = (int) ActivationBehaviour;
            Entity.ProfileCategoryId = Category.Entity.Id;

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
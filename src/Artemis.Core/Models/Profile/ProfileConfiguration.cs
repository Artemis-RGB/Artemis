using System.Collections.Generic;
using System.Linq;
using Artemis.Core.Modules;
using Artemis.Core.Services;
using Artemis.Storage.Entities.Profile;

namespace Artemis.Core
{
    public class ProfileConfiguration : IStorageModel
    {
        internal ProfileConfiguration(string name, string icon, ProfileCategory category)
        {
            Name = name;
            Icon = icon;
            Category = category;
            Entity = new ProfileConfigurationEntity();
        }

        internal ProfileConfiguration(ProfileCategory category, ProfileConfigurationEntity entity)
        {
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
        ///     alongside any activation requirements of the <see cref="Modules" />
        /// </summary>
        public DataModelConditionGroup? ActivationCondition { get; set; }

        /// <summary>
        ///     Gets a list of modules this profile uses
        /// </summary>
        public List<Module> Modules { get; } = new();

        internal ProfileConfigurationEntity Entity { get; }

        internal void LoadModules(List<Module> enabledModules)
        {
            Modules.Clear();
            foreach (Module enabledModule in enabledModules.Where(m => Entity.Modules.Contains(m.Id))) 
                Modules.Add(enabledModule);
        }

        internal void SaveModules()
        {
            Entity.Modules.Clear();
            foreach (Module module in Modules) 
                Entity.Modules.Add(module.Id);
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
            {
                Entity.ActivationCondition = null;
            }
        }

        #endregion
    }
}
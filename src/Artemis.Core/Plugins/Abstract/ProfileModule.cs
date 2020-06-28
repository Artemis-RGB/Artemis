using System;
using Artemis.Core.Exceptions;
using Artemis.Core.Models.Profile;
using Artemis.Core.Models.Surface;
using Artemis.Core.Plugins.Abstract.DataModels;
using Artemis.Core.Plugins.Abstract.DataModels.Attributes;
using Artemis.Core.Plugins.Models;
using SkiaSharp;

namespace Artemis.Core.Plugins.Abstract
{
    /// <summary>
    ///     Allows you to add support for new games/applications while utilizing Artemis' profile engine and your own data model
    /// </summary>
    public abstract class ProfileModule<T> : ProfileModule where T : DataModel
    {
        /// <summary>
        ///     The data model driving this module
        /// </summary>
        public T DataModel
        {
            get => (T) InternalDataModel;
            internal set => InternalDataModel = value;
        }

        /// <summary>
        ///     Gets or sets whether this module must also expand the main data model
        ///     <para>
        ///         Note: If expanding the main data model is all you want your plugin to do, create a
        ///         <see cref="BaseDataModelExpansion" /> plugin instead.
        ///     </para>
        /// </summary>
        public bool ExpandsDataModel
        {
            get => InternalExpandsMainDataModel;
            set => InternalExpandsMainDataModel = value;
        }

        /// <summary>
        ///     Override to provide your own data model description. By default this returns a description matching your plugin
        ///     name and description
        /// </summary>
        /// <returns></returns>
        public virtual DataModelPropertyAttribute GetDataModelDescription()
        {
            return new DataModelPropertyAttribute {Name = PluginInfo.Name, Description = PluginInfo.Description};
        }
        
        internal override void InternalEnablePlugin()
        {
            DataModel = Activator.CreateInstance<T>();
            DataModel.PluginInfo = PluginInfo;
            DataModel.DataModelDescription = GetDataModelDescription();
            base.InternalEnablePlugin();
        }

        internal override void InternalDisablePlugin()
        {
            DataModel = null;
            base.InternalDisablePlugin();
        }
    }

    /// <summary>
    ///     Allows you to add support for new games/applications while utilizing Artemis' profile engine
    /// </summary>
    public abstract class ProfileModule : Module
    {
        public Profile ActiveProfile { get; private set; }

        /// <inheritdoc />
        public override void Update(double deltaTime)
        {
            lock (this)
            {
                // Update the profile
                ActiveProfile?.Update(deltaTime);
            }
        }

        /// <inheritdoc />
        public override void Render(double deltaTime, ArtemisSurface surface, SKCanvas canvas, SKImageInfo canvasInfo)
        {
            lock (this)
            {
                // Render the profile
                ActiveProfile?.Render(deltaTime, canvas, canvasInfo);
            }
        }

        internal void ChangeActiveProfile(Profile profile, ArtemisSurface surface)
        {
            if (profile != null && profile.PluginInfo != PluginInfo)
                throw new ArtemisCoreException($"Cannot activate a profile of plugin {profile.PluginInfo} on a module of plugin {PluginInfo}.");
            lock (this)
            {
                if (profile == ActiveProfile)
                    return;

                ActiveProfile?.Deactivate();

                ActiveProfile = profile;
                ActiveProfile?.Activate(surface);
            }

            OnActiveProfileChanged();
        }

        #region Events

        public event EventHandler ActiveProfileChanged;

        protected virtual void OnActiveProfileChanged()
        {
            ActiveProfileChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}
using System;
using System.Drawing;
using Artemis.Core.Exceptions;
using Artemis.Core.Models.Profile;
using Artemis.Core.Models.Surface;
using Artemis.Core.Plugins.Models;

namespace Artemis.Core.Plugins.Abstract
{
    public abstract class ProfileModule : Module
    {
        protected ProfileModule(PluginInfo pluginInfo) : base(pluginInfo)
        {
        }

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
        public override void Render(double deltaTime, ArtemisSurface surface, Graphics graphics)
        {
            lock (this)
            {
                // Render the profile
                ActiveProfile?.Render(deltaTime, surface, graphics);
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
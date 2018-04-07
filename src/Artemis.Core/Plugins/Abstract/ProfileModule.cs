using System;
using Artemis.Core.Plugins.Interfaces;
using Artemis.Core.ProfileElements;
using RGB.NET.Core;

namespace Artemis.Core.Plugins.Abstract
{
    public abstract class ProfileModule : IModule
    {
        public Profile ActiveProfile { get; private set; }

        /// <inheritdoc />
        public abstract Type ViewModelType { get; }

        /// <inheritdoc />
        public abstract bool ExpandsMainDataModel { get; }

        /// <inheritdoc />
        public void LoadPlugin()
        {
            // Load and activate the last active profile
        }

        public void ChangeActiveProfile(Profile profile)
        {
            lock (this)
            {
                if (profile == null)
                    throw new ArgumentNullException(nameof(profile));
                
                ActiveProfile?.Deactivate();

                ActiveProfile = profile;
                ActiveProfile.Activate();
            }
        }

        /// <inheritdoc />
        public virtual void Update(double deltaTime)
        {
            lock (this)
            {
                // Update the profile
                ActiveProfile?.Update(deltaTime);
            }
        }

        /// <inheritdoc />
        public virtual void Render(double deltaTime, RGBSurface surface)
        {
            lock (this)
            {
                // Render the profile
                ActiveProfile?.Render(deltaTime, surface);
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
        }
    }
}
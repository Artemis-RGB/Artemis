using System;
using System.Drawing;
using Artemis.Core.Plugins.Interfaces;
using Artemis.Core.ProfileElements;
using RGB.NET.Core;
using Stylet;

namespace Artemis.Core.Plugins.Abstract
{
    public abstract class ProfileModule : IModule
    {
        public Profile ActiveProfile { get; private set; }

        /// <inheritdoc />
        public abstract string DisplayName { get; }

        /// <inheritdoc />
        public abstract bool ExpandsMainDataModel { get; }

        /// <inheritdoc />
        public void EnablePlugin()
        {
            // Load and activate the last active profile
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
        public virtual void Render(double deltaTime, RGBSurface surface, Graphics graphics)
        {
            lock (this)
            {
                // Render the profile
                ActiveProfile?.Render(deltaTime, surface, graphics);
            }
        }

        /// <inheritdoc />
        public abstract IScreen GetMainViewModel();

        /// <inheritdoc />
        public void Dispose()
        {
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
    }
}
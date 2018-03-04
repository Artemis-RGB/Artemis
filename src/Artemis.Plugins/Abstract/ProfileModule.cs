using System;
using Artemis.Plugins.Interfaces;

namespace Artemis.Plugins.Abstract
{
    public abstract class ProfileModule : IModule
    {
        /// <inheritdoc />
        public abstract Type ViewModelType { get; }

        /// <inheritdoc />
        public abstract bool ExpandsMainDataModel { get; }

        /// <inheritdoc />
        public void LoadPlugin()
        {
            // Load and activate the last active profile
        }

        /// <inheritdoc />
        public void UnloadPlugin()
        {
            // Unload the last active profile
        }

        /// <inheritdoc />
        public virtual void Update(double deltaTime)
        {
            // Update the profile
        }

        /// <inheritdoc />
        public virtual void Render(double deltaTime)
        {
            // Render the profile
        }
    }
}
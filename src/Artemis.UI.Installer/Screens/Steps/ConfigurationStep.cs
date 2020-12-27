using Stylet;

namespace Artemis.UI.Installer.Screens.Steps
{
    public abstract class ConfigurationStep : Screen
    {
        /// <inheritdoc />
        protected ConfigurationStep()
        {
        }

        /// <inheritdoc />
        protected ConfigurationStep(IModelValidator validator) : base(validator)
        {
        }

        public abstract int Order { get; }
    }
}
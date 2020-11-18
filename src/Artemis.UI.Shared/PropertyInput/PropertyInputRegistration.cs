using System;
using Artemis.Core;
using Artemis.UI.Shared.Services;

namespace Artemis.UI.Shared
{
    /// <summary>
    ///     Represents a property input registration, registered through <see cref="IProfileEditorService.RegisterPropertyInput"/>
    /// </summary>
    public class PropertyInputRegistration
    {
        private readonly IProfileEditorService _profileEditorService;

        internal PropertyInputRegistration(IProfileEditorService profileEditorService, Plugin plugin, Type supportedType, Type viewModelType)
        {
            _profileEditorService = profileEditorService;
            Plugin = plugin;
            SupportedType = supportedType;
            ViewModelType = viewModelType;

            if (Plugin != Constants.CorePlugin)
                Plugin.Disabled += InstanceOnDisabled;
        }

        /// <summary>
        ///     Gets the plugin that registered the property input
        /// </summary>
        public Plugin Plugin { get; }

        /// <summary>
        ///     Gets the type supported by the property input
        /// </summary>
        public Type SupportedType { get; }

        /// <summary>
        ///     Gets the view model type of the property input
        /// </summary>
        public Type ViewModelType { get; }

        internal void Unsubscribe()
        {
            if (Plugin != Constants.CorePlugin)
                Plugin.Disabled -= InstanceOnDisabled;
        }

        private void InstanceOnDisabled(object? sender, EventArgs e)
        {
            // Profile editor service will call Unsubscribe
            _profileEditorService.RemovePropertyInput(this);
        }
    }
}
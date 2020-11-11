using System;
using Artemis.Core;
using Artemis.UI.Shared.Services;

namespace Artemis.UI.Shared
{
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

        public Plugin Plugin { get; }
        public Type SupportedType { get; }
        public Type ViewModelType { get; }

        internal void Unsubscribe()
        {
            if (Plugin != Constants.CorePlugin)
                Plugin.Disabled -= InstanceOnDisabled;
        }

        private void InstanceOnDisabled(object sender, EventArgs e)
        {
            // Profile editor service will call Unsubscribe
            _profileEditorService.RemovePropertyInput(this);
        }
    }
}
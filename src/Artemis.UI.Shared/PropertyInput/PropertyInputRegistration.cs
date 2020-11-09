using System;
using Artemis.Core;
using Artemis.UI.Shared.Services;

namespace Artemis.UI.Shared
{
    public class PropertyInputRegistration
    {
        private readonly IProfileEditorService _profileEditorService;

        internal PropertyInputRegistration(IProfileEditorService profileEditorService, PluginInfo pluginInfo, Type supportedType, Type viewModelType)
        {
            _profileEditorService = profileEditorService;
            PluginInfo = pluginInfo;
            SupportedType = supportedType;
            ViewModelType = viewModelType;

            if (PluginInfo != Constants.CorePluginInfo)
                PluginInfo.Instance.Disabled += InstanceOnDisabled;
        }

        public PluginInfo PluginInfo { get; }
        public Type SupportedType { get; }
        public Type ViewModelType { get; }

        internal void Unsubscribe()
        {
            if (PluginInfo != Constants.CorePluginInfo)
                PluginInfo.Instance.Disabled -= InstanceOnDisabled;
        }

        private void InstanceOnDisabled(object sender, EventArgs e)
        {
            // Profile editor service will call Unsubscribe
            _profileEditorService.RemovePropertyInput(this);
        }
    }
}
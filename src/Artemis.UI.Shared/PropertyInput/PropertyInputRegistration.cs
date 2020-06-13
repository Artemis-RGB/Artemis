using System;
using Artemis.Core;
using Artemis.Core.Plugins.Models;
using Artemis.UI.Shared.Services.Interfaces;

namespace Artemis.UI.Shared.PropertyInput
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
                PluginInfo.Instance.PluginDisabled += InstanceOnPluginDisabled;
        }

        public PluginInfo PluginInfo { get; }
        public Type SupportedType { get; }
        public Type ViewModelType { get; }

        internal void Unsubscribe()
        {
            if (PluginInfo != Constants.CorePluginInfo)
                PluginInfo.Instance.PluginDisabled -= InstanceOnPluginDisabled;
        }

        internal void Remove()
        {
            // It'll call Unsubscribe for us
            _profileEditorService.RemovePropertyInput(this);
        }

        private void InstanceOnPluginDisabled(object sender, EventArgs e)
        {
            Remove();
        }
    }
}
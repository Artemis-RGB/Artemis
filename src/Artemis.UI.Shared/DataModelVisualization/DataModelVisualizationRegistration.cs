using System;
using System.Collections.Generic;
using Artemis.Core;
using Artemis.Core.Plugins;
using Artemis.UI.Shared.Services;

namespace Artemis.UI.Shared.DataModelVisualization
{
    public class DataModelVisualizationRegistration
    {
        private readonly IDataModelVisualizationService _dataModelVisualizationService;

        public DataModelVisualizationRegistration(IDataModelVisualizationService dataModelVisualizationService,
            RegistrationType registrationType,
            PluginInfo pluginInfo,
            Type supportedType,
            Type viewModelType)
        {
            _dataModelVisualizationService = dataModelVisualizationService;
            RegistrationType = registrationType;
            PluginInfo = pluginInfo;
            SupportedType = supportedType;
            ViewModelType = viewModelType;

            if (PluginInfo != Constants.CorePluginInfo)
                PluginInfo.Instance.PluginDisabled += InstanceOnPluginDisabled;
        }

        public RegistrationType RegistrationType { get; }
        public PluginInfo PluginInfo { get; }
        public Type SupportedType { get; }
        public Type ViewModelType { get; }

        public IReadOnlyCollection<Type> CompatibleConversionTypes { get; internal set; }

        internal void Unsubscribe()
        {
            if (PluginInfo != Constants.CorePluginInfo)
                PluginInfo.Instance.PluginDisabled -= InstanceOnPluginDisabled;
        }

        private void InstanceOnPluginDisabled(object sender, EventArgs e)
        {
            if (RegistrationType == RegistrationType.Input)
                _dataModelVisualizationService.RemoveDataModelInput(this);
            else if (RegistrationType == RegistrationType.Display)
                _dataModelVisualizationService.RemoveDataModelDisplay(this);
        }
    }

    public enum RegistrationType
    {
        Display,
        Input
    }
}
using System;
using System.Collections.Generic;
using Artemis.Core;
using Artemis.UI.Shared.Services;

namespace Artemis.UI.Shared
{
    public class DataModelVisualizationRegistration
    {
        private readonly IDataModelUIService _dataModelUIService;

        public DataModelVisualizationRegistration(IDataModelUIService dataModelUIService,
            RegistrationType registrationType,
            Plugin plugin,
            Type supportedType,
            Type viewModelType)
        {
            _dataModelUIService = dataModelUIService;
            RegistrationType = registrationType;
            Plugin = plugin;
            SupportedType = supportedType;
            ViewModelType = viewModelType;

            if (Plugin != Constants.CorePlugin)
                Plugin.Disabled += InstanceOnDisabled;
        }

        public RegistrationType RegistrationType { get; }
        public Plugin Plugin { get; }
        public Type SupportedType { get; }
        public Type ViewModelType { get; }

        public IReadOnlyCollection<Type> CompatibleConversionTypes { get; internal set; }

        internal void Unsubscribe()
        {
            if (Plugin != Constants.CorePlugin)
                Plugin.Disabled -= InstanceOnDisabled;
        }

        private void InstanceOnDisabled(object sender, EventArgs e)
        {
            if (RegistrationType == RegistrationType.Input)
                _dataModelUIService.RemoveDataModelInput(this);
            else if (RegistrationType == RegistrationType.Display)
                _dataModelUIService.RemoveDataModelDisplay(this);
        }
    }

    public enum RegistrationType
    {
        Display,
        Input
    }
}
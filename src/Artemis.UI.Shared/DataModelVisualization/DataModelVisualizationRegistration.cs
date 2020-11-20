using System;
using System.Collections.Generic;
using Artemis.Core;
using Artemis.UI.Shared.Services;

namespace Artemis.UI.Shared
{
    /// <summary>
    ///     Represents a layer brush registered through
    ///     <see cref="IDataModelUIService.RegisterDataModelInput{T}" /> or
    ///     <see cref="IDataModelUIService.RegisterDataModelDisplay{T}" />
    /// </summary>
    public class DataModelVisualizationRegistration
    {
        private readonly IDataModelUIService _dataModelUIService;

        internal DataModelVisualizationRegistration(IDataModelUIService dataModelUIService,
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

        /// <summary>
        ///     Gets the type of registration, either a display or an input
        /// </summary>
        public RegistrationType RegistrationType { get; }

        /// <summary>
        ///     Gets the plugin that registered the visualization
        /// </summary>
        public Plugin Plugin { get; }

        /// <summary>
        ///     Gets the type supported by the visualization
        /// </summary>
        public Type SupportedType { get; }

        /// <summary>
        ///     Gets the view model type of the visualization
        /// </summary>
        public Type ViewModelType { get; }

        /// <summary>
        ///     Gets a read only collection of types this visualization can convert to and from
        /// </summary>
        public IReadOnlyCollection<Type>? CompatibleConversionTypes { get; internal set; }

        internal void Unsubscribe()
        {
            if (Plugin != Constants.CorePlugin)
                Plugin.Disabled -= InstanceOnDisabled;
        }

        private void InstanceOnDisabled(object? sender, EventArgs e)
        {
            if (RegistrationType == RegistrationType.Input)
                _dataModelUIService.RemoveDataModelInput(this);
            else if (RegistrationType == RegistrationType.Display)
                _dataModelUIService.RemoveDataModelDisplay(this);
        }
    }

    /// <summary>
    ///     Represents a type of data model visualization registration
    /// </summary>
    public enum RegistrationType
    {
        /// <summary>
        ///     A visualization used for displaying values
        /// </summary>
        Display,

        /// <summary>
        ///     A visualization used for inputting values
        /// </summary>
        Input
    }
}
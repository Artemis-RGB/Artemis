using System;
using System.Collections.Generic;
using Artemis.Core.Plugins;
using Artemis.Core.Plugins.DataModelExpansions.Attributes;
using Artemis.UI.Shared.DataModelVisualization;
using Artemis.UI.Shared.DataModelVisualization.Shared;

namespace Artemis.UI.Shared.Services.Interfaces
{
    public interface IDataModelUIService : IArtemisSharedUIService
    {
        DataModelPropertiesViewModel GetMainDataModelVisualization();
        DataModelPropertiesViewModel GetPluginDataModelVisualization(Plugin plugin);

        /// <summary>
        ///     Determines whether the given plugin expands the main data model
        /// </summary>
        /// <param name="plugin"></param>
        /// <returns></returns>
        bool GetPluginExtendsDataModel(Plugin plugin);

        DataModelVisualizationRegistration RegisterDataModelInput<T>(PluginInfo pluginInfo, IReadOnlyCollection<Type> compatibleConversionTypes) where T : DataModelInputViewModel;
        DataModelVisualizationRegistration RegisterDataModelDisplay<T>(PluginInfo pluginInfo) where T : DataModelDisplayViewModel;
        void RemoveDataModelInput(DataModelVisualizationRegistration registration);
        void RemoveDataModelDisplay(DataModelVisualizationRegistration registration);

        DataModelDisplayViewModel GetDataModelDisplayViewModel(Type propertyType);
        DataModelInputViewModel GetDataModelInputViewModel(Type propertyType, DataModelPropertyAttribute description, object initialValue, Action<object, bool> updateCallback);
        IReadOnlyCollection<DataModelVisualizationRegistration> RegisteredDataModelEditors { get; }
        IReadOnlyCollection<DataModelVisualizationRegistration> RegisteredDataModelDisplays { get; }
    }
}
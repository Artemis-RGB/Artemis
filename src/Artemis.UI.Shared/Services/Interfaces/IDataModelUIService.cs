using System;
using System.Collections.Generic;
using Artemis.Core;
using Artemis.Core.DataModelExpansions;
using Artemis.Core.Modules;
using Artemis.UI.Shared.Input;

namespace Artemis.UI.Shared.Services
{
    public interface IDataModelUIService : IArtemisSharedUIService
    {
        IReadOnlyCollection<DataModelVisualizationRegistration> RegisteredDataModelEditors { get; }
        IReadOnlyCollection<DataModelVisualizationRegistration> RegisteredDataModelDisplays { get; }
        DataModelPropertiesViewModel GetMainDataModelVisualization();
        DataModelPropertiesViewModel GetPluginDataModelVisualization(Plugin plugin, bool includeMainDataModel);

        DataModelVisualizationRegistration RegisterDataModelInput<T>(PluginInfo pluginInfo, IReadOnlyCollection<Type> compatibleConversionTypes) where T : DataModelInputViewModel;
        DataModelVisualizationRegistration RegisterDataModelDisplay<T>(PluginInfo pluginInfo) where T : DataModelDisplayViewModel;
        void RemoveDataModelInput(DataModelVisualizationRegistration registration);
        void RemoveDataModelDisplay(DataModelVisualizationRegistration registration);

        DataModelDisplayViewModel GetDataModelDisplayViewModel(Type propertyType);
        DataModelInputViewModel GetDataModelInputViewModel(Type propertyType, DataModelPropertyAttribute description, object initialValue, Action<object, bool> updateCallback);

        DataModelDynamicViewModel GetDynamicSelectionViewModel(Module module);
        DataModelStaticViewModel GetStaticInputViewModel(Type targetType);
    }
}
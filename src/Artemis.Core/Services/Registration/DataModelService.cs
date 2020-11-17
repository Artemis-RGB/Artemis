using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.Core.DataModelExpansions;
using Artemis.Core.Modules;

namespace Artemis.Core.Services
{
    internal class DataModelService : IDataModelService
    {
        public DataModelService(IPluginManagementService pluginManagementService)
        {
            // Add data models of already loaded plugins
            foreach (Module module in pluginManagementService.GetFeaturesOfType<Module>().Where(p => p.IsEnabled && p.InternalDataModel != null))
                AddModuleDataModel(module);
            foreach (BaseDataModelExpansion dataModelExpansion in pluginManagementService.GetFeaturesOfType<BaseDataModelExpansion>().Where(p => p.IsEnabled))
                AddDataModelExpansionDataModel(dataModelExpansion);

            // Add data models of new plugins when they get enabled
            pluginManagementService.PluginFeatureEnabled += OnPluginFeatureEnabled;
        }


        public DataModelRegistration RegisterDataModel(DataModel dataModel)
        {
            if (dataModel == null)
                throw new ArgumentNullException(nameof(dataModel));
            return DataModelStore.Add(dataModel);
        }

        public void RemoveDataModel(DataModelRegistration registration)
        {
            if (registration == null)
                throw new ArgumentNullException(nameof(registration));
            DataModelStore.Remove(registration);
        }

        public List<DataModel> GetDataModels()
        {
            return DataModelStore.GetAll().Select(d => d.DataModel).ToList();
        }

        public T? GetDataModel<T>() where T : DataModel
        {
            return (T?) DataModelStore.GetAll().FirstOrDefault(d => d.DataModel is T)?.DataModel;
        }

        public DataModel? GetPluginDataModel(PluginFeature pluginFeature)
        {
            return DataModelStore.Get(pluginFeature.Id)?.DataModel;
        }
        
        private void OnPluginFeatureEnabled(object? sender, PluginFeatureEventArgs e)
        {
            if (e.PluginFeature is Module module)
                AddModuleDataModel(module);
            else if (e.PluginFeature is BaseDataModelExpansion dataModelExpansion)
                AddDataModelExpansionDataModel(dataModelExpansion);
        }

        private void AddModuleDataModel(Module module)
        {
            if (module.InternalDataModel == null)
                throw new ArtemisCoreException("Cannot add module data model that is not enabled");
            if (module.InternalDataModel.DataModelDescription == null)
                throw new ArtemisPluginFeatureException(module, "Module overrides GetDataModelDescription but returned null");

            module.InternalDataModel.IsExpansion = module.InternalExpandsMainDataModel;
            RegisterDataModel(module.InternalDataModel);
        }

        private void AddDataModelExpansionDataModel(BaseDataModelExpansion dataModelExpansion)
        {
            if (dataModelExpansion.InternalDataModel == null)
                throw new ArtemisCoreException("Cannot add data model expansion that is not enabled");
            if (dataModelExpansion.InternalDataModel.DataModelDescription == null)
                throw new ArtemisPluginFeatureException(dataModelExpansion, "Data model expansion overrides GetDataModelDescription but returned null");

            dataModelExpansion.InternalDataModel.IsExpansion = true;
            RegisterDataModel(dataModelExpansion.InternalDataModel);
        }
    }
}
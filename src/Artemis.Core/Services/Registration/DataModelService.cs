using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.Core.DataModelExpansions;
using Artemis.Core.Modules;

namespace Artemis.Core.Services
{
    internal class DataModelService : IDataModelService
    {
        public DataModelService(IPluginService pluginService)
        {
            // Add data models of already loaded plugins
            foreach (var module in pluginService.GetPluginsOfType<Module>())
                AddModuleDataModel(module);
            foreach (var dataModelExpansion in pluginService.GetPluginsOfType<BaseDataModelExpansion>())
                AddDataModelExpansionDataModel(dataModelExpansion);

            // Add data models of new plugins when they get enabled
            pluginService.PluginEnabled += PluginServiceOnPluginEnabled;
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

        public T GetDataModel<T>() where T : DataModel
        {
            return (T) DataModelStore.GetAll().FirstOrDefault(d => d.DataModel is T)?.DataModel;
        }

        public DataModel GetPluginDataModel(Plugin plugin)
        {
            return DataModelStore.Get(plugin.PluginInfo.Guid)?.DataModel;
        }

        public DataModel GetPluginDataModel(Guid pluginGuid)
        {
            return DataModelStore.Get(pluginGuid)?.DataModel;
        }

        private void PluginServiceOnPluginEnabled(object sender, PluginEventArgs e)
        {
            if (e.PluginInfo.Instance is Module module)
                AddModuleDataModel(module);
            else if (e.PluginInfo.Instance is BaseDataModelExpansion dataModelExpansion)
                AddDataModelExpansionDataModel(dataModelExpansion);
        }

        private void AddModuleDataModel(Module module)
        {
            if (module.InternalDataModel == null)
                return;

            if (module.InternalDataModel.DataModelDescription == null)
                throw new ArtemisPluginException(module.PluginInfo, "Module overrides GetDataModelDescription but returned null");

            module.InternalDataModel.IsExpansion = module.InternalExpandsMainDataModel;
            RegisterDataModel(module.InternalDataModel);
        }

        private void AddDataModelExpansionDataModel(BaseDataModelExpansion dataModelExpansion)
        {
            if (dataModelExpansion.InternalDataModel.DataModelDescription == null)
                throw new ArtemisPluginException(dataModelExpansion.PluginInfo, "Data model expansion overrides GetDataModelDescription but returned null");

            dataModelExpansion.InternalDataModel.IsExpansion = true;
            RegisterDataModel(dataModelExpansion.InternalDataModel);
        }
    }
}
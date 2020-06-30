using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Artemis.Core.Events;
using Artemis.Core.Exceptions;
using Artemis.Core.Plugins.Abstract;
using Artemis.Core.Plugins.Abstract.DataModels;
using Artemis.Core.Plugins.Exceptions;
using Artemis.Core.Services.Interfaces;

namespace Artemis.Core.Services
{
    /// <summary>
    ///     Provides access to the main data model
    /// </summary>
    public class DataModelService : IDataModelService
    {
        private readonly List<DataModel> _dataModelExpansions;
        private readonly IPluginService _pluginService;

        internal DataModelService(IPluginService pluginService)
        {
            _pluginService = pluginService;
            _dataModelExpansions = new List<DataModel>();

            _pluginService.PluginEnabled += PluginServiceOnPluginEnabled;
            _pluginService.PluginDisabled += PluginServiceOnPluginDisabled;

            foreach (var module in _pluginService.GetPluginsOfType<Module>().Where(m => m.InternalExpandsMainDataModel))
                AddModuleDataModel(module);
            foreach (var dataModelExpansion in _pluginService.GetPluginsOfType<BaseDataModelExpansion>())
                AddDataModelExpansionDataModel(dataModelExpansion);
        }

        public ReadOnlyCollection<DataModel> DataModelExpansions
        {
            get
            {
                lock (_dataModelExpansions)
                {
                    return new List<DataModel>(_dataModelExpansions).AsReadOnly();
                }
            }
        }

        public void AddExpansion(DataModel dataModelExpansion)
        {
            lock (_dataModelExpansions)
            {
                _dataModelExpansions.Add(dataModelExpansion);
                // TODO SpoinkyNL 3-3-2018: Initialize the expansion and fire an event
            }
        }

        public void RemoveExpansion(DataModel dataModelExpansion)
        {
            lock (_dataModelExpansions)
            {
                if (!_dataModelExpansions.Contains(dataModelExpansion))
                    throw new ArtemisCoreException("Cannot remove a data model expansion that wasn't previously added.");

                // TODO SpoinkyNL 3-3-2018: Dispose the expansion and fire an event
                _dataModelExpansions.Remove(dataModelExpansion);
            }
        }

        public DataModel GetPluginDataModel(Plugin plugin)
        {
            if (plugin is Module module)
                return module.InternalDataModel;
            if (plugin is BaseDataModelExpansion dataModelExpansion)
                return dataModelExpansion.InternalDataModel;
            return null;
        }

        private void PluginServiceOnPluginEnabled(object sender, PluginEventArgs e)
        {
            if (e.PluginInfo.Instance is Module module && module.InternalExpandsMainDataModel)
                AddModuleDataModel(module);
            else if (e.PluginInfo.Instance is BaseDataModelExpansion dataModelExpansion)
                AddDataModelExpansionDataModel(dataModelExpansion);
        }

        private void AddDataModelExpansionDataModel(BaseDataModelExpansion dataModelExpansion)
        {
            if (dataModelExpansion.InternalDataModel.DataModelDescription == null)
                throw new ArtemisPluginException(dataModelExpansion.PluginInfo, "Data model expansion overrides GetDataModelDescription but returned null");
            
            AddExpansion(dataModelExpansion.InternalDataModel);
        }

        private void AddModuleDataModel(Module module)
        {
            if (module.InternalDataModel.DataModelDescription == null)
                throw new ArtemisPluginException(module.PluginInfo, "Module overrides GetDataModelDescription but returned null");
            
            AddExpansion(module.InternalDataModel);
        }

        private void PluginServiceOnPluginDisabled(object sender, PluginEventArgs e)
        {
            // Remove all data models related to the plugin
            lock (_dataModelExpansions)
            {
                var toRemove = _dataModelExpansions.Where(d => d.PluginInfo == e.PluginInfo).ToList();
                foreach (var dataModel in toRemove)
                    _dataModelExpansions.Remove(dataModel);
            }
        }
    }
}
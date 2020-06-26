using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Artemis.Core.Events;
using Artemis.Core.Exceptions;
using Artemis.Core.Models;
using Artemis.Core.Plugins.Abstract;
using Artemis.Core.Plugins.Abstract.DataModels;
using Artemis.Core.Plugins.Abstract.DataModels.Attributes;
using Artemis.Core.Plugins.Exceptions;
using Artemis.Core.Services.Interfaces;

namespace Artemis.Core.Services
{
    /// <summary>
    ///     Provides access to the main data model
    /// </summary>
    public class DataModelService : IDataModelService
    {
        private readonly IPluginService _pluginService;
        private readonly List<DataModel> _dataModelExpansions;

        internal DataModelService(IPluginService pluginService)
        {
            _pluginService = pluginService;
            _dataModelExpansions = new List<DataModel>();

            _pluginService.PluginEnabled += PluginServiceOnPluginEnabled;
            _pluginService.PluginDisabled += PluginServiceOnPluginDisabled;
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

        private void PluginServiceOnPluginEnabled(object sender, PluginEventArgs e)
        {
            if (e.PluginInfo.Instance is Module module && module.InternalExpandsMainDataModel)
            {
                if (module.InternalDataModel.DataModelDescription == null)
                {
                    module.InternalDataModel.DataModelDescription = module.InternalGetDataModelDescription();
                    if (module.InternalDataModel.DataModelDescription == null)
                        throw new ArtemisPluginException(module.PluginInfo, "Module overrides GetDataModelDescription but returned null");
                }

                _dataModelExpansions.Add(module.InternalDataModel);
            }
            else if (e.PluginInfo.Instance is BaseDataModelExpansion dataModelExpansion)
            {
                if (dataModelExpansion.InternalDataModel.DataModelDescription == null)
                {
                    dataModelExpansion.InternalDataModel.DataModelDescription = dataModelExpansion.GetDataModelDescription();
                    if (dataModelExpansion.InternalDataModel.DataModelDescription == null)
                        throw new ArtemisPluginException(dataModelExpansion.PluginInfo, "Data model expansion overrides GetDataModelDescription but returned null");
                }

                _dataModelExpansions.Add(dataModelExpansion.InternalDataModel);
            }
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
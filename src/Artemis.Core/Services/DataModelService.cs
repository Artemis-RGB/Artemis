using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Artemis.Core.Events;
using Artemis.Core.Exceptions;
using Artemis.Core.Extensions;
using Artemis.Core.Models.Profile.Conditions;
using Artemis.Core.Models.Profile.Conditions.Operators;
using Artemis.Core.Plugins.Abstract;
using Artemis.Core.Plugins.Abstract.DataModels;
using Artemis.Core.Plugins.Exceptions;
using Artemis.Core.Plugins.Models;
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
        private readonly List<DisplayConditionOperator> _registeredConditionOperators;

        internal DataModelService(IPluginService pluginService)
        {
            _pluginService = pluginService;
            _dataModelExpansions = new List<DataModel>();
            _registeredConditionOperators = new List<DisplayConditionOperator>();

            _pluginService.PluginEnabled += PluginServiceOnPluginEnabled;
            _pluginService.PluginDisabled += PluginServiceOnPluginDisabled;

            RegisterBuiltInConditionOperators();

            foreach (var module in _pluginService.GetPluginsOfType<Module>().Where(m => m.InternalExpandsMainDataModel))
                AddModuleDataModel(module);
            foreach (var dataModelExpansion in _pluginService.GetPluginsOfType<BaseDataModelExpansion>())
                AddDataModelExpansionDataModel(dataModelExpansion);
        }

        public IReadOnlyCollection<DisplayConditionOperator> RegisteredConditionOperators
        {
            get
            {
                lock (_registeredConditionOperators)
                {
                    return _registeredConditionOperators.AsReadOnly();
                }
            }
        }

        public IReadOnlyCollection<DataModel> DataModelExpansions
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

        public DataModel GetPluginDataModelByGuid(Guid pluginGuid)
        {
            var pluginInfo = _pluginService.GetAllPluginInfo().FirstOrDefault(i => i.Guid == pluginGuid);
            if (pluginInfo == null || !pluginInfo.Enabled)
                return null;

            return GetPluginDataModel(pluginInfo.Instance);
        }

        public bool GetPluginExtendsDataModel(Plugin plugin)
        {
            if (plugin is Module module)
                return module.InternalExpandsMainDataModel;
            if (plugin is BaseDataModelExpansion)
                return true;

            return false;
        }


        public void RegisterConditionOperator(PluginInfo pluginInfo, DisplayConditionOperator displayConditionOperator)
        {
            if (pluginInfo == null)
                throw new ArgumentNullException(nameof(pluginInfo));
            if (displayConditionOperator == null)
                throw new ArgumentNullException(nameof(displayConditionOperator));

            lock (_registeredConditionOperators)
            {
                if (_registeredConditionOperators.Contains(displayConditionOperator))
                    return;

                displayConditionOperator.Register(pluginInfo, this);
                _registeredConditionOperators.Add(displayConditionOperator);
            }
        }

        public void RemoveConditionOperator(DisplayConditionOperator displayConditionOperator)
        {
            if (displayConditionOperator == null)
                throw new ArgumentNullException(nameof(displayConditionOperator));

            lock (_registeredConditionOperators)
            {
                if (!_registeredConditionOperators.Contains(displayConditionOperator))
                    return;

                displayConditionOperator.Unsubscribe();
                _registeredConditionOperators.Remove(displayConditionOperator);
            }
        }

        public List<DisplayConditionOperator> GetCompatibleConditionOperators(Type type)
        {
            lock (_registeredConditionOperators)
            {
                if (type == null)
                    return new List<DisplayConditionOperator>(_registeredConditionOperators);

                var candidates = _registeredConditionOperators.Where(c => c.CompatibleTypes.Any(t => t.IsCastableFrom(type))).ToList();

                // If there are multiple operators with the same description, use the closest match
                foreach (var displayConditionOperators in candidates.GroupBy(c => c.Description).Where(g => g.Count() > 1).ToList())
                {
                    var bestCandidate = displayConditionOperators.OrderByDescending(c => c.CompatibleTypes.Contains(type)).FirstOrDefault();
                    foreach (var displayConditionOperator in displayConditionOperators)
                    {
                        if (displayConditionOperator != bestCandidate)
                            candidates.Remove(displayConditionOperator);
                    }
                }

                return candidates;
            }
        }

        public DisplayConditionOperator GetConditionOperator(Guid operatorPluginGuid, string operatorType)
        {
            return RegisteredConditionOperators.FirstOrDefault(o => o.PluginInfo.Guid == operatorPluginGuid && o.GetType().Name == operatorType);
        }

        private void RegisterBuiltInConditionOperators()
        {
            // General usage for any type
            RegisterConditionOperator(Constants.CorePluginInfo, new EqualsConditionOperator());
            RegisterConditionOperator(Constants.CorePluginInfo, new NotEqualConditionOperator());

            // Numeric operators
            RegisterConditionOperator(Constants.CorePluginInfo, new LessThanConditionOperator());
            RegisterConditionOperator(Constants.CorePluginInfo, new GreaterThanConditionOperator());
            RegisterConditionOperator(Constants.CorePluginInfo, new LessThanOrEqualConditionOperator());
            RegisterConditionOperator(Constants.CorePluginInfo, new GreaterThanOrEqualConditionOperator());

            // String operators
            RegisterConditionOperator(Constants.CorePluginInfo, new StringEqualsConditionOperator());
            RegisterConditionOperator(Constants.CorePluginInfo, new StringNotEqualConditionOperator());
            RegisterConditionOperator(Constants.CorePluginInfo, new StringContainsConditionOperator());
            RegisterConditionOperator(Constants.CorePluginInfo, new StringNotContainsConditionOperator());
            RegisterConditionOperator(Constants.CorePluginInfo, new StringStartsWithConditionOperator());
            RegisterConditionOperator(Constants.CorePluginInfo, new StringEndsWithConditionOperator());
            RegisterConditionOperator(Constants.CorePluginInfo, new StringNullConditionOperator());
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
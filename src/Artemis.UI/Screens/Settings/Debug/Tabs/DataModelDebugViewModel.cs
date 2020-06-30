using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Artemis.Core.Events;
using Artemis.Core.Services.Interfaces;
using Artemis.UI.DataModelVisualization;
using Artemis.UI.Services;
using Stylet;

namespace Artemis.UI.Screens.Settings.Debug.Tabs
{
    public class DataModelDebugViewModel : Screen
    {
        private readonly IDataModelVisualizationService _dataModelVisualizationService;
        private readonly IPluginService _pluginService;
        private readonly Timer _updateTimer;
        private bool _isModuleFilterEnabled;
        private Core.Plugins.Abstract.Module _selectedModule;

        public DataModelDebugViewModel(IDataModelVisualizationService dataModelVisualizationService, IPluginService pluginService)
        {
            _dataModelVisualizationService = dataModelVisualizationService;
            _pluginService = pluginService;
            _updateTimer = new Timer(500);
            _updateTimer.Elapsed += (sender, args) => MainDataModel.Update();

            DisplayName = "Data model";
        }

        public DataModelViewModel MainDataModel { get; set; }

        public string PropertySearch { get; set; }
        public List<Core.Plugins.Abstract.Module> Modules { get; set; }

        public Core.Plugins.Abstract.Module SelectedModule
        {
            get => _selectedModule;
            set
            {
                _selectedModule = value;
                GetDataModel();
            }
        }

        public bool IsModuleFilterEnabled
        {
            get => _isModuleFilterEnabled;
            set
            {
                _isModuleFilterEnabled = value;
                
                if (!IsModuleFilterEnabled)
                    SelectedModule = null;
                else
                    GetDataModel();
            }
        }

        protected override void OnActivate()
        {
            GetDataModel();
            _updateTimer.Start();
            _pluginService.PluginEnabled += PluginServiceOnPluginToggled;
            _pluginService.PluginDisabled += PluginServiceOnPluginToggled;

            PopulateModules();
        }

        protected override void OnDeactivate()
        {
            _updateTimer.Stop();
            _pluginService.PluginEnabled -= PluginServiceOnPluginToggled;
            _pluginService.PluginDisabled -= PluginServiceOnPluginToggled;
        }

        private void GetDataModel()
        {
            MainDataModel = SelectedModule != null 
                ? _dataModelVisualizationService.GetPluginDataModelVisualization(SelectedModule) 
                : _dataModelVisualizationService.GetMainDataModelVisualization();
        }

        private void PluginServiceOnPluginToggled(object? sender, PluginEventArgs e)
        {
            PopulateModules();
        }

        private void PopulateModules()
        {
            Modules = _pluginService.GetPluginsOfType<Core.Plugins.Abstract.Module>().Where(p => p.Enabled).ToList();
        }
    }
}
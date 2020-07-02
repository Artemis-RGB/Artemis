using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Artemis.Core.Events;
using Artemis.Core.Services.Interfaces;
using Artemis.UI.Services;
using Artemis.UI.Shared.DataModelVisualization.Shared;
using Artemis.UI.Shared.Services;
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
        private DataModelViewModel _mainDataModel;
        private string _propertySearch;
        private List<Core.Plugins.Abstract.Module> _modules;

        public DataModelDebugViewModel(IDataModelVisualizationService dataModelVisualizationService, IPluginService pluginService)
        {
            _dataModelVisualizationService = dataModelVisualizationService;
            _pluginService = pluginService;
            _updateTimer = new Timer(500);
            _updateTimer.Elapsed += (sender, args) => MainDataModel.Update();

            DisplayName = "Data model";
        }

        public DataModelViewModel MainDataModel
        {
            get => _mainDataModel;
            set => SetAndNotify(ref _mainDataModel, value);
        }

        public string PropertySearch
        {
            get => _propertySearch;
            set => SetAndNotify(ref _propertySearch, value);
        }

        public List<Core.Plugins.Abstract.Module> Modules
        {
            get => _modules;
            set => SetAndNotify(ref _modules, value);
        }

        public Core.Plugins.Abstract.Module SelectedModule
        {
            get => _selectedModule;
            set
            {
                if (!SetAndNotify(ref _selectedModule, value)) return;
                GetDataModel();
            }
        }

        public bool IsModuleFilterEnabled
        {
            get => _isModuleFilterEnabled;
            set
            {
                SetAndNotify(ref _isModuleFilterEnabled, value);

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
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Artemis.Core.Events;
using Artemis.Core.Services.Interfaces;
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
        private DataModelPropertiesViewModel _mainDataModel;
        private List<Core.Plugins.Modules.Module> _modules;
        private string _propertySearch;
        private Core.Plugins.Modules.Module _selectedModule;

        public DataModelDebugViewModel(IDataModelVisualizationService dataModelVisualizationService, IPluginService pluginService)
        {
            _dataModelVisualizationService = dataModelVisualizationService;
            _pluginService = pluginService;
            _updateTimer = new Timer(500);

            DisplayName = "Data model";
        }

        public DataModelPropertiesViewModel MainDataModel
        {
            get => _mainDataModel;
            set => SetAndNotify(ref _mainDataModel, value);
        }

        public string PropertySearch
        {
            get => _propertySearch;
            set => SetAndNotify(ref _propertySearch, value);
        }

        public List<Core.Plugins.Modules.Module> Modules
        {
            get => _modules;
            set => SetAndNotify(ref _modules, value);
        }

        public Core.Plugins.Modules.Module SelectedModule
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
            _updateTimer.Elapsed += OnUpdateTimerOnElapsed;
            _pluginService.PluginEnabled += PluginServiceOnPluginToggled;
            _pluginService.PluginDisabled += PluginServiceOnPluginToggled;

            PopulateModules();
        }

        protected override void OnDeactivate()
        {
            _updateTimer.Stop();
            _updateTimer.Elapsed -= OnUpdateTimerOnElapsed;
            _pluginService.PluginEnabled -= PluginServiceOnPluginToggled;
            _pluginService.PluginDisabled -= PluginServiceOnPluginToggled;
        }

        private void OnUpdateTimerOnElapsed(object sender, ElapsedEventArgs args)
        {
            MainDataModel.Update(_dataModelVisualizationService);
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
            Modules = _pluginService.GetPluginsOfType<Core.Plugins.Modules.Module>().Where(p => p.Enabled).ToList();
        }
    }
}
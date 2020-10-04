using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Artemis.Core;
using Artemis.Core.Modules;
using Artemis.Core.Services;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using Stylet;

namespace Artemis.UI.Screens.Settings.Debug.Tabs
{
    public class DataModelDebugViewModel : Screen
    {
        private readonly IDataModelUIService _dataModelUIService;
        private readonly IPluginService _pluginService;
        private readonly Timer _updateTimer;
        private bool _isModuleFilterEnabled;
        private DataModelPropertiesViewModel _mainDataModel;
        private List<Module> _modules;
        private string _propertySearch;
        private Module _selectedModule;

        public DataModelDebugViewModel(IDataModelUIService dataModelUIService, IPluginService pluginService)
        {
            _dataModelUIService = dataModelUIService;
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

        public List<Module> Modules
        {
            get => _modules;
            set => SetAndNotify(ref _modules, value);
        }

        public Module SelectedModule
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

            base.OnActivate();
        }

        protected override void OnDeactivate()
        {
            _updateTimer.Stop();
            _updateTimer.Elapsed -= OnUpdateTimerOnElapsed;
            _pluginService.PluginEnabled -= PluginServiceOnPluginToggled;
            _pluginService.PluginDisabled -= PluginServiceOnPluginToggled;

            base.OnDeactivate();
        }

        private void OnUpdateTimerOnElapsed(object sender, ElapsedEventArgs args)
        {
            lock (MainDataModel)
            {
                MainDataModel.Update(_dataModelUIService);
            }
        }

        private void GetDataModel()
        {
            MainDataModel = SelectedModule != null
                ? _dataModelUIService.GetPluginDataModelVisualization(SelectedModule, false)
                : _dataModelUIService.GetMainDataModelVisualization();
        }

        private void PluginServiceOnPluginToggled(object? sender, PluginEventArgs e)
        {
            PopulateModules();
        }

        private void PopulateModules()
        {
            Modules = _pluginService.GetPluginsOfType<Module>().Where(p => p.Enabled).ToList();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Timers;
using Artemis.Core;
using Artemis.Core.Modules;
using Artemis.Core.Services;
using Artemis.UI.Shared;
using Artemis.UI.Shared.DataModelVisualization.Shared;
using Artemis.UI.Shared.Services;
using Artemis.UI.Shared.Services.Interfaces;
using DynamicData;
using ReactiveUI;

namespace Artemis.UI.Screens.Debugger.Tabs.DataModel
{
    public class DataModelDebugViewModel : ActivatableViewModelBase, IRoutableViewModel
    {
        private readonly IDataModelUIService _dataModelUIService;
        private readonly IPluginManagementService _pluginManagementService;
        private readonly Timer _updateTimer;

        private bool _isModuleFilterEnabled;
        private DataModelPropertiesViewModel? _mainDataModel;
        private string? _propertySearch;
        private Module? _selectedModule;
        private bool _slowUpdates;

        public DataModelDebugViewModel(IScreen hostScreen, IDataModelUIService dataModelUIService, IPluginManagementService pluginManagementService)
        {
            _dataModelUIService = dataModelUIService;
            _pluginManagementService = pluginManagementService;
            _updateTimer = new Timer(25);
            _updateTimer.Elapsed += UpdateTimerOnElapsed;

            HostScreen = hostScreen;
            Modules = new ObservableCollection<Module>();

            this.WhenActivated(disposables =>
            {
                Observable.FromEventPattern<PluginFeatureEventArgs>(x => pluginManagementService.PluginFeatureEnabled += x, x => pluginManagementService.PluginFeatureEnabled -= x)
                    .Subscribe(d => PluginFeatureToggled(d.EventArgs.PluginFeature))
                    .DisposeWith(disposables);
                Observable.FromEventPattern<PluginFeatureEventArgs>(x => pluginManagementService.PluginFeatureDisabled += x, x => pluginManagementService.PluginFeatureDisabled -= x)
                    .Subscribe(d => PluginFeatureToggled(d.EventArgs.PluginFeature))
                    .DisposeWith(disposables);

                GetDataModel();
                _updateTimer.Start();
                Disposable.Create(() => _updateTimer.Stop()).DisposeWith(disposables);
            });
        }

        public string UrlPathSegment => "data-model";
        public IScreen HostScreen { get; }

        public DataModelPropertiesViewModel? MainDataModel
        {
            get => _mainDataModel;
            set => this.RaiseAndSetIfChanged(ref _mainDataModel, value);
        }

        public string? PropertySearch
        {
            get => _propertySearch;
            set => this.RaiseAndSetIfChanged(ref _propertySearch, value);
        }

        public bool SlowUpdates
        {
            get => _slowUpdates;
            set
            {
                this.RaiseAndSetIfChanged(ref _slowUpdates, value);
                _updateTimer.Interval = _slowUpdates ? 500 : 25;
            }
        }

        public ObservableCollection<Module> Modules { get; }

        public Module? SelectedModule
        {
            get => _selectedModule;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedModule, value);
                GetDataModel();
            }
        }

        public bool IsModuleFilterEnabled
        {
            get => _isModuleFilterEnabled;
            set
            {
                this.RaiseAndSetIfChanged(ref _isModuleFilterEnabled, value);

                if (!IsModuleFilterEnabled)
                    SelectedModule = null;
                else
                    GetDataModel();
            }
        }
        private void UpdateTimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            if (MainDataModel == null)
                return;

            lock (MainDataModel)
            {
                MainDataModel.Update(_dataModelUIService, new DataModelUpdateConfiguration(true));
            }
        }

        private void PluginFeatureToggled(PluginFeature pluginFeature)
        {
            if (pluginFeature is Module)
                PopulateModules();
        }

        private void GetDataModel()
        {
            MainDataModel = SelectedModule != null
                ? _dataModelUIService.GetPluginDataModelVisualization(new List<Module>() { SelectedModule }, false)
                : _dataModelUIService.GetMainDataModelVisualization();
        }

        private void PopulateModules()
        {
            Modules.Clear();
            Modules.AddRange(_pluginManagementService.GetFeaturesOfType<Module>().Where(p => p.IsEnabled).OrderBy(m => m.Info.Name));

            if (MainDataModel == null)
                return;

            if (SelectedModule == null)
            {
                if (MainDataModel != null)
                    _dataModelUIService.UpdateModules(MainDataModel);
            }
            else if (!SelectedModule.IsEnabled)
                SelectedModule = null;
        }
    }
}
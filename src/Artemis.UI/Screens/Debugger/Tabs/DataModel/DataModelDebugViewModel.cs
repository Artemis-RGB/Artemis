using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using Artemis.Core;
using Artemis.Core.Modules;
using Artemis.Core.Services;
using Artemis.UI.Shared;
using Artemis.UI.Shared.DataModelVisualization.Shared;
using Artemis.UI.Shared.Services;
using Avalonia.Threading;
using DynamicData;
using PropertyChanged.SourceGenerator;
using ReactiveUI;

namespace Artemis.UI.Screens.Debugger.DataModel;

public partial class DataModelDebugViewModel : ActivatableViewModelBase
{
    private readonly IDataModelUIService _dataModelUIService;
    private readonly IPluginManagementService _pluginManagementService;
    private readonly DispatcherTimer _updateTimer;
    private bool _isModuleFilterEnabled;
    private Module? _selectedModule;
    private bool _slowUpdates;
    [Notify] private DataModelPropertiesViewModel? _mainDataModel;
    [Notify] private string? _propertySearch;

    public DataModelDebugViewModel(IDataModelUIService dataModelUIService, IPluginManagementService pluginManagementService)
    {
        _dataModelUIService = dataModelUIService;
        _pluginManagementService = pluginManagementService;
        _updateTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(40), DispatcherPriority.Background, (_, _) => Update());

        DisplayName = "Data Model";
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
            Disposable.Create(() =>
            {
                _updateTimer.Stop();
                MainDataModel?.Dispose();
                MainDataModel = null;
            }).DisposeWith(disposables);
        });
    }

    public bool SlowUpdates
    {
        get => _slowUpdates;
        set
        {
            RaiseAndSetIfChanged(ref _slowUpdates, value);
            _updateTimer.Interval = TimeSpan.FromMilliseconds(_slowUpdates ? 500 : 25);
        }
    }

    public ObservableCollection<Module> Modules { get; }

    public Module? SelectedModule
    {
        get => _selectedModule;
        set
        {
            RaiseAndSetIfChanged(ref _selectedModule, value);
            GetDataModel();
        }
    }

    public bool IsModuleFilterEnabled
    {
        get => _isModuleFilterEnabled;
        set
        {
            RaiseAndSetIfChanged(ref _isModuleFilterEnabled, value);

            if (!IsModuleFilterEnabled)
                SelectedModule = null;
            else
                GetDataModel();
        }
    }

    private void Update()
    {
        if (MainDataModel == null)
            return;

        lock (MainDataModel)
        {
            MainDataModel.Update(_dataModelUIService, new DataModelUpdateConfiguration(true, false));
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
            ? _dataModelUIService.GetPluginDataModelVisualization(new List<Module> {SelectedModule}, false)
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
        {
            SelectedModule = null;
        }
    }
}
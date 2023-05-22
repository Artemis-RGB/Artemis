using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Timers;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.DryIoc.Factories;
using Artemis.UI.Extensions;
using Artemis.UI.Screens.VisualScripting;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using Artemis.UI.Shared.Services.ProfileEditor;
using Artemis.UI.Shared.Services.ProfileEditor.Commands;
using Avalonia.Threading;
using ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor.Properties.DataBinding;

public class DataBindingViewModel : ActivatableViewModelBase
{
    private readonly PluginSetting<bool> _alwaysApplyDataBindings;
    private readonly IProfileEditorService _profileEditorService;
    private readonly IWindowService _windowService;
    private ObservableAsPropertyHelper<bool>? _dataBindingEnabled;
    private bool _editorOpen;
    private ObservableAsPropertyHelper<ILayerProperty?>? _layerProperty;
    private ObservableAsPropertyHelper<NodeScriptViewModel?>? _nodeScriptViewModel;
    private bool _playing;

    public DataBindingViewModel(IProfileEditorService profileEditorService, INodeVmFactory nodeVmFactory, IWindowService windowService, ISettingsService settingsService)
    {
        _profileEditorService = profileEditorService;
        _windowService = windowService;
        _alwaysApplyDataBindings = settingsService.GetSetting("ProfileEditor.AlwaysApplyDataBindings", false);

        this.WhenActivated(d =>
        {
            _layerProperty = profileEditorService.LayerProperty.ToProperty(this, vm => vm.LayerProperty).DisposeWith(d);
            _nodeScriptViewModel = profileEditorService.LayerProperty
                .Select(p => p != null ? p.BaseDataBinding.AsObservable() : Observable.Never<IDataBinding>())
                .Switch()
                .Select(b => b.IsEnabled ? nodeVmFactory.NodeScriptViewModel((NodeScript) b.Script, false) : null)
                .ToProperty(this, vm => vm.NodeScriptViewModel)
                .DisposeWith(d);
            _dataBindingEnabled = profileEditorService.LayerProperty
                .Select(p => p != null ? p.BaseDataBinding.AsObservable() : Observable.Never<IDataBinding>())
                .Switch()
                .Select(b => b.IsEnabled)
                .ToProperty(this, vm => vm.DataBindingEnabled)
                .DisposeWith(d);
            _profileEditorService.Playing.CombineLatest(_profileEditorService.SuspendedEditing).Subscribe(tuple => _playing = tuple.First || tuple.Second).DisposeWith(d);

            Timer updateTimer = new(TimeSpan.FromMilliseconds(25.0 / 1000));
            Timer saveTimer = new(TimeSpan.FromMinutes(2));

            updateTimer.Elapsed += (_, _) => Update();
            saveTimer.Elapsed += (_, _) => Save();
            updateTimer.Start();
            saveTimer.Start();

            updateTimer.DisposeWith(d);
            saveTimer.DisposeWith(d);
        });
    }

    public ILayerProperty? LayerProperty => _layerProperty?.Value;
    public NodeScriptViewModel? NodeScriptViewModel => _nodeScriptViewModel?.Value;

    public bool DataBindingEnabled
    {
        get => _dataBindingEnabled?.Value ?? false;
        set
        {
            if (LayerProperty != null)
                _profileEditorService.ExecuteCommand(new ChangeDataBindingEnabled(LayerProperty, value));
        }
    }

    public async Task OpenEditor()
    {
        if (LayerProperty == null || !LayerProperty.BaseDataBinding.IsEnabled)
            return;

        try
        {
            _editorOpen = true;
            await _windowService.ShowDialogAsync<NodeScriptWindowViewModel, bool>(LayerProperty.BaseDataBinding.Script);
            await _profileEditorService.SaveProfileAsync();
        }
        finally
        {
            _editorOpen = false;
        }
    }

    private void Update()
    {
        // If playing the data binding will already be updated, no need to do it here
        if (_playing || !_alwaysApplyDataBindings.Value)
            return;

        LayerProperty?.UpdateDataBinding();
    }

    private void Save()
    {
        if (!_editorOpen)
            _profileEditorService.SaveProfile();
    }
}
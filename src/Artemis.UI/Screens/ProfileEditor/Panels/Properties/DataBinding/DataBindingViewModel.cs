using System.Reactive.Linq;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.UI.Extensions;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Screens.VisualScripting;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using Artemis.UI.Shared.Services.ProfileEditor;
using Artemis.UI.Shared.Services.ProfileEditor.Commands;
using Avalonia.Controls.Mixins;
using ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor.Properties.DataBinding;

public class DataBindingViewModel : ActivatableViewModelBase
{
    private readonly IProfileEditorService _profileEditorService;
    private readonly IWindowService _windowService;
    private ObservableAsPropertyHelper<bool>? _dataBindingEnabled;
    private ObservableAsPropertyHelper<ILayerProperty?>? _layerProperty;
    private ObservableAsPropertyHelper<NodeScriptViewModel?>? _nodeScriptViewModel;

    public DataBindingViewModel(IProfileEditorService profileEditorService, INodeVmFactory nodeVmFactory, IWindowService windowService)
    {
        _profileEditorService = profileEditorService;
        _windowService = windowService;

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
        if (LayerProperty != null && LayerProperty.BaseDataBinding.IsEnabled)
            await _windowService.ShowDialogAsync<NodeScriptWindowViewModel, bool>(("nodeScript", LayerProperty.BaseDataBinding.Script));
    }
}
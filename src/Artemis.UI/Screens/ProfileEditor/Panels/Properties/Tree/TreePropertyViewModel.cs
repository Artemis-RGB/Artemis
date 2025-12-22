using System;
using System.Reactive;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using Artemis.Core;
using Artemis.UI.Extensions;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services.ProfileEditor;
using Artemis.UI.Shared.Services.ProfileEditor.Commands;
using Artemis.UI.Shared.Services.PropertyInput;
using ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor.Properties.Tree;

internal class TreePropertyViewModel<T> : ActivatableViewModelBase, ITreePropertyViewModel
{
    private readonly IProfileEditorService _profileEditorService;
    private ObservableAsPropertyHelper<bool>? _dataBindingEnabled;
    private ObservableAsPropertyHelper<bool>? _isCurrentlySelected;
    private TimeSpan _time;

    public TreePropertyViewModel(LayerProperty<T> layerProperty, PropertyViewModel propertyViewModel, IProfileEditorService profileEditorService, IPropertyInputService propertyInputService)
    {
        _profileEditorService = profileEditorService;

        LayerProperty = layerProperty;
        PropertyViewModel = propertyViewModel;
        PropertyInputViewModel = propertyInputService.CreatePropertyInputViewModel(LayerProperty);

        this.WhenActivated(d =>
        {
            _profileEditorService.Time.Subscribe(t => _time = t).DisposeWith(d);
            _isCurrentlySelected = _profileEditorService.LayerProperty.Select(l => l == LayerProperty).ToProperty(this, vm => vm.IsCurrentlySelected).DisposeWith(d);
            _dataBindingEnabled = LayerProperty.BaseDataBinding.AsObservable().Select(b => b.IsEnabled).ToProperty(this, vm => vm.DataBindingEnabled).DisposeWith(d);
            this.WhenAnyValue(vm => vm.LayerProperty.KeyframesEnabled).Subscribe(_ => this.RaisePropertyChanged(nameof(KeyframesEnabled))).DisposeWith(d);
        });

        ResetToDefault = ReactiveCommand.Create(ExecuteResetToDefault, Observable.Return(LayerProperty.DefaultValue != null));
    }

    public bool IsCurrentlySelected => _isCurrentlySelected?.Value ?? false;
    public LayerProperty<T> LayerProperty { get; }
    public PropertyViewModel PropertyViewModel { get; }
    public PropertyInputViewModel<T>? PropertyInputViewModel { get; }
    public ReactiveCommand<Unit, Unit> ResetToDefault { get; }

    public bool KeyframesEnabled
    {
        get => LayerProperty.KeyframesEnabled;
        set => UpdateKeyframesEnabled(value);
    }

    private void UpdateKeyframesEnabled(bool value)
    {
        if (value == LayerProperty.KeyframesEnabled)
            return;

        _profileEditorService.ExecuteCommand(new ToggleLayerPropertyKeyframes<T>(LayerProperty, value, _time));
    }

    private void ExecuteResetToDefault()
    {
        _profileEditorService.ExecuteCommand(new ResetLayerProperty<T>(LayerProperty));
    }

    public bool DataBindingEnabled => _dataBindingEnabled?.Value ?? false;

    public ILayerProperty BaseLayerProperty => LayerProperty;

    public double GetDepth()
    {
        int depth = 0;
        LayerPropertyGroup? current = LayerProperty.LayerPropertyGroup;
        while (current != null)
        {
            depth++;
            current = current.Parent;
        }

        return depth;
    }

    /// <inheritdoc />
    public void ToggleCurrentLayerProperty()
    {
        _profileEditorService.ChangeCurrentLayerProperty(IsCurrentlySelected ? null : LayerProperty);
    }
}
using System;
using System.Reactive;
using System.Reactive.Linq;
using Artemis.Core;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services.ProfileEditor;
using Artemis.UI.Shared.Services.ProfileEditor.Commands;
using Artemis.UI.Shared.Services.PropertyInput;
using Avalonia.Controls.Mixins;
using ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor.Properties.Tree;

internal class TreePropertyViewModel<T> : ActivatableViewModelBase, ITreePropertyViewModel
{
    private readonly IProfileEditorService _profileEditorService;
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
            this.WhenAnyValue(vm => vm.LayerProperty.KeyframesEnabled).Subscribe(_ => this.RaisePropertyChanged(nameof(KeyframesEnabled))).DisposeWith(d);
        });

        ResetToDefault = ReactiveCommand.Create(ExecuteResetToDefault, Observable.Return(LayerProperty.DefaultValue != null));
    }

    private void ExecuteResetToDefault()
    {
        _profileEditorService.ExecuteCommand(new ResetLayerProperty<T>(LayerProperty));
    }

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

    public ILayerProperty BaseLayerProperty => LayerProperty;
    public bool HasDataBinding => LayerProperty.HasDataBinding;

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
}


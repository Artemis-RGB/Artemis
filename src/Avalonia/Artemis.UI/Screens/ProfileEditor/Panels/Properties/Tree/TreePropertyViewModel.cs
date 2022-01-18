using System;
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

    public TreePropertyViewModel(LayerProperty<T> layerProperty, PropertyViewModel propertyViewModel, IProfileEditorService profileEditorService,
        IPropertyInputService propertyInputService)
    {
        _profileEditorService = profileEditorService;

        LayerProperty = layerProperty;
        PropertyViewModel = propertyViewModel;
        PropertyInputViewModel = propertyInputService.CreatePropertyInputViewModel(LayerProperty);

        this.WhenActivated(d => this.WhenAnyValue(vm => vm.LayerProperty.KeyframesEnabled).Subscribe(_ => this.RaisePropertyChanged(nameof(KeyframesEnabled))).DisposeWith(d));
    }

    public LayerProperty<T> LayerProperty { get; }
    public PropertyViewModel PropertyViewModel { get; }
    public PropertyInputViewModel<T>? PropertyInputViewModel { get; }

    public bool KeyframesEnabled
    {
        get => LayerProperty.KeyframesEnabled;
        set => UpdateKeyframesEnabled(value);
    }

    private void UpdateKeyframesEnabled(bool value)
    {
        if (value == LayerProperty.KeyframesEnabled)
            return;

        _profileEditorService.ExecuteCommand(new ToggleLayerPropertyKeyframes<T>(LayerProperty, value));
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
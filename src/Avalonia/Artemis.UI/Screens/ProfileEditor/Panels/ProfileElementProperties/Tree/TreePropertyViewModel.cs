using Artemis.Core;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services.PropertyInput;

namespace Artemis.UI.Screens.ProfileEditor.ProfileElementProperties.Tree;

internal class TreePropertyViewModel<T> : ActivatableViewModelBase, ITreePropertyViewModel
{
    public TreePropertyViewModel(LayerProperty<T> layerProperty, ProfileElementPropertyViewModel profileElementPropertyViewModel, IPropertyInputService propertyInputService)
    {
        LayerProperty = layerProperty;
        ProfileElementPropertyViewModel = profileElementPropertyViewModel;
        PropertyInputViewModel = propertyInputService.CreatePropertyInputViewModel(LayerProperty);
    }

    public LayerProperty<T> LayerProperty { get; }
    public ProfileElementPropertyViewModel ProfileElementPropertyViewModel { get; }
    public PropertyInputViewModel<T>? PropertyInputViewModel { get; }

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
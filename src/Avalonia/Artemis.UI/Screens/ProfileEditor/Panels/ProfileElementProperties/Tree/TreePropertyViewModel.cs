using Artemis.Core;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services.PropertyInput;

namespace Artemis.UI.Screens.ProfileEditor.ProfileElementProperties.Tree;

internal class TreePropertyViewModel<T> : ActivatableViewModelBase
{
    public TreePropertyViewModel(LayerProperty<T> layerProperty, ProfileElementPropertyViewModel layerPropertyViewModel, IPropertyInputService propertyInputService)
    {
        LayerProperty = layerProperty;
        LayerPropertyViewModel = layerPropertyViewModel;
        PropertyInputViewModel = propertyInputService.CreatePropertyInputViewModel(LayerProperty);
    }

    public LayerProperty<T> LayerProperty { get; }
    public ProfileElementPropertyViewModel LayerPropertyViewModel { get; }
    public PropertyInputViewModel<T>? PropertyInputViewModel { get; }
}
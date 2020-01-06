using System.Collections.Generic;
using Artemis.Core.Models.Profile;
using Stylet;

namespace Artemis.UI.Screens.Module.ProfileEditor.LayerProperties
{
    public class LayerPropertyViewModel : PropertyChangedBase
    {
        public LayerPropertyViewModel(LayerProperty layerProperty, LayerPropertyViewModel parent)
        {
            LayerProperty = layerProperty;
            Parent = parent;
            Children = new List<LayerPropertyViewModel>();

            foreach (var child in layerProperty.Children)
                Children.Add(new LayerPropertyViewModel(child, this));
        }

        public LayerProperty LayerProperty { get; }

        public LayerPropertyViewModel Parent { get; }
        public List<LayerPropertyViewModel> Children { get; set; }

        public bool IsExpanded { get; set; }
    }
}
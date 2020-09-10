using System;
using Artemis.Core;
using Artemis.UI.Screens.ProfileEditor.LayerProperties.Tree;
using Ninject;
using Ninject.Parameters;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.LayerProperties
{
    public class LayerPropertyViewModel : PropertyChangedBase, IDisposable
    {
        public LayerPropertyViewModel(ILayerProperty layerProperty, IKernel kernel)
        {
            LayerProperty = layerProperty;

            var parameter = new ConstructorArgument("layerProperty", LayerProperty);
            var treeViewModelType = typeof(LayerPropertyTreeViewModel<>).MakeGenericType(layerProperty.GetType().GetGenericArguments());
            var timelineViewModelType = typeof(LayerPropertyTimelineViewModel<>).MakeGenericType(layerProperty.GetType().GetGenericArguments());

            LayerPropertyTreeViewModel = (ILayerPropertyTreeViewModel) kernel.Get(treeViewModelType, parameter);
            LayerPropertyTimelineViewModel = (ILayerPropertyTimelineViewModel) kernel.Get(timelineViewModelType, parameter);
        }

        public ILayerProperty LayerProperty { get; }
        public ILayerPropertyTreeViewModel LayerPropertyTreeViewModel { get; }
        public ILayerPropertyTimelineViewModel LayerPropertyTimelineViewModel { get; }

        public bool IsVisible { get; set; }
        public bool IsExpanded { get; set; }

        public void Dispose()
        {
            LayerPropertyTreeViewModel?.Dispose();
            LayerPropertyTimelineViewModel?.Dispose();
        }
    }
}
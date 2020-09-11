using System;
using Artemis.Core;
using Artemis.UI.Screens.ProfileEditor.LayerProperties.Timeline;
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
            var treeViewModelType = typeof(TreePropertyViewModel<>).MakeGenericType(layerProperty.GetType().GetGenericArguments());
            var timelineViewModelType = typeof(TimelinePropertyViewModel<>).MakeGenericType(layerProperty.GetType().GetGenericArguments());

            TreePropertyViewModel = (ITreePropertyViewModel) kernel.Get(treeViewModelType, parameter);
            TimelinePropertyViewModel = (ITimelinePropertyViewModel) kernel.Get(timelineViewModelType, parameter);
        }

        public ILayerProperty LayerProperty { get; }
        public ITreePropertyViewModel TreePropertyViewModel { get; }
        public ITimelinePropertyViewModel TimelinePropertyViewModel { get; }

        public bool IsVisible { get; set; }
        public bool IsExpanded { get; set; }

        public void Dispose()
        {
            TreePropertyViewModel?.Dispose();
            TimelinePropertyViewModel?.Dispose();
        }
    }
}
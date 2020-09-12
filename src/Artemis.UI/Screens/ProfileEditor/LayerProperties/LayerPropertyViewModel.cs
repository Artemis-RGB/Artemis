using System;
using Artemis.Core;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Screens.ProfileEditor.LayerProperties.Timeline;
using Artemis.UI.Screens.ProfileEditor.LayerProperties.Tree;
using Ninject;
using Ninject.Parameters;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.LayerProperties
{
    public class LayerPropertyViewModel : PropertyChangedBase, IDisposable
    {
        public LayerPropertyViewModel(ILayerProperty layerProperty, IPropertyVmFactory propertyVmFactory)
        {
            LayerProperty = layerProperty;

            TreePropertyViewModel = propertyVmFactory.TreePropertyViewModel(layerProperty, this);
            TimelinePropertyViewModel = propertyVmFactory.TimelinePropertyViewModel(layerProperty, this);
        }

        public ILayerProperty LayerProperty { get; }
        public ITreePropertyViewModel TreePropertyViewModel { get; }
        public ITimelinePropertyViewModel TimelinePropertyViewModel { get; }

        public bool IsVisible => !LayerProperty.IsHidden;
        
        public void Dispose()
        {
            TreePropertyViewModel?.Dispose();
            TimelinePropertyViewModel?.Dispose();
        }
    }
}
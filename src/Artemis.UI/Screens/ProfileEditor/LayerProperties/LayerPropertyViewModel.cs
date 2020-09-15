using System;
using Artemis.Core;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Screens.ProfileEditor.LayerProperties.Timeline;
using Artemis.UI.Screens.ProfileEditor.LayerProperties.Tree;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.LayerProperties
{
    public class LayerPropertyViewModel : PropertyChangedBase, IDisposable
    {
        private bool _isVisible;

        public LayerPropertyViewModel(ILayerProperty layerProperty, IPropertyVmFactory propertyVmFactory)
        {
            LayerProperty = layerProperty;

            TreePropertyViewModel = propertyVmFactory.TreePropertyViewModel(layerProperty, this);
            TimelinePropertyViewModel = propertyVmFactory.TimelinePropertyViewModel(layerProperty, this);
        }

        public ILayerProperty LayerProperty { get; }
        public ITreePropertyViewModel TreePropertyViewModel { get; }
        public ITimelinePropertyViewModel TimelinePropertyViewModel { get; }

        public bool IsVisible
        {
            get => _isVisible;
            set => SetAndNotify(ref _isVisible, value);
        }

        public bool IsExpanded => false;
        
        public void Dispose()
        {
            TreePropertyViewModel?.Dispose();
            TimelinePropertyViewModel?.Dispose();
        }
    }
}
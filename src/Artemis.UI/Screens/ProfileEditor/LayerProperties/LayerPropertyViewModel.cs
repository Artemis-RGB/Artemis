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
        private bool _isHighlighted;
        private bool _isExpanded;

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

        public bool IsHighlighted
        {
            get => _isHighlighted;
            set => SetAndNotify(ref _isHighlighted, value);
        }

        public bool IsExpanded
        {
            get => _isExpanded;
            set => SetAndNotify(ref _isExpanded, value);
        }

        public void Dispose()
        {
            TreePropertyViewModel?.Dispose();
            TimelinePropertyViewModel?.Dispose();
        }
    }
}
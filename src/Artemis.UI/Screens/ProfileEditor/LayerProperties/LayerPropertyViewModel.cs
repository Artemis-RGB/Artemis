using Artemis.Core;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Screens.ProfileEditor.LayerProperties.Timeline;
using Artemis.UI.Screens.ProfileEditor.LayerProperties.Tree;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.LayerProperties
{
    public sealed class LayerPropertyViewModel : Screen
    {
        private bool _isExpanded;
        private bool _isHighlighted;
        private bool _isVisible;
        private ITimelinePropertyViewModel _timelinePropertyViewModel;
        private ITreePropertyViewModel _treePropertyViewModel;


        public LayerPropertyViewModel(ILayerProperty layerProperty, IPropertyVmFactory propertyVmFactory)
        {
            LayerProperty = layerProperty;

            TreePropertyViewModel = propertyVmFactory.TreePropertyViewModel(LayerProperty, this);
            TreePropertyViewModel.ConductWith(this);
            TimelinePropertyViewModel = propertyVmFactory.TimelinePropertyViewModel(LayerProperty, this);
            TimelinePropertyViewModel.ConductWith(this);
        }

        public ILayerProperty LayerProperty { get; }

        public ITreePropertyViewModel TreePropertyViewModel
        {
            get => _treePropertyViewModel;
            set => SetAndNotify(ref _treePropertyViewModel, value);
        }

        public ITimelinePropertyViewModel TimelinePropertyViewModel
        {
            get => _timelinePropertyViewModel;
            set => SetAndNotify(ref _timelinePropertyViewModel, value);
        }

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
    }
}
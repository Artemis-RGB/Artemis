using System.Collections.Generic;
using System.Linq;
using Artemis.UI.Ninject.Factories;
using Stylet;

namespace Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.Timeline
{
    public class PropertyTrackViewModel : Screen
    {
        private readonly IPropertyTrackKeyframeVmFactory _propertyTrackKeyframeVmFactory;

        public PropertyTrackViewModel(PropertyTimelineViewModel propertyTimelineViewModel, LayerPropertyViewModel layerPropertyViewModel,
            IPropertyTrackKeyframeVmFactory propertyTrackKeyframeVmFactory)
        {
            _propertyTrackKeyframeVmFactory = propertyTrackKeyframeVmFactory;
            PropertyTimelineViewModel = propertyTimelineViewModel;
            LayerPropertyViewModel = layerPropertyViewModel;
            KeyframeViewModels = new BindableCollection<PropertyTrackKeyframeViewModel>();

            PopulateKeyframes();
            UpdateKeyframes(PropertyTimelineViewModel.LayerPropertiesViewModel.PixelsPerSecond);

            LayerPropertyViewModel.ExpandedStateChanged += (sender, args) => UpdateMustDisplay();
            LayerPropertyViewModel.LayerProperty.VisibilityChanged += (sender, args) => UpdateMustDisplay();
            UpdateMustDisplay();
        }


        public PropertyTimelineViewModel PropertyTimelineViewModel { get; }
        public LayerPropertyViewModel LayerPropertyViewModel { get; }
        public BindableCollection<PropertyTrackKeyframeViewModel> KeyframeViewModels { get; set; }
        public bool MustDisplay { get; set; }

        private void UpdateMustDisplay()
        {
            var expandedTest = LayerPropertyViewModel.Parent;
            while (expandedTest != null)
            {
                if (!expandedTest.IsExpanded)
                {
                    MustDisplay = false;
                    return;
                }
                expandedTest = expandedTest.Parent;
            }

            var visibilityTest = LayerPropertyViewModel.LayerProperty;
            while (visibilityTest != null)
            {
                if (visibilityTest.IsHidden)
                {
                    MustDisplay = false;
                    return;
                }
                visibilityTest = visibilityTest.Parent;
            }

            MustDisplay = true;
        }

        public void PopulateKeyframes()
        {
            // Remove old keyframes
            KeyframeViewModels.RemoveRange(KeyframeViewModels.ToList().Where(vm => !LayerPropertyViewModel.LayerProperty.UntypedKeyframes.Contains(vm.Keyframe)));

            // Add new keyframes
            KeyframeViewModels.AddRange(
                LayerPropertyViewModel.LayerProperty.UntypedKeyframes
                    .Where(k => KeyframeViewModels.All(vm => vm.Keyframe != k))
                    .Select(k => _propertyTrackKeyframeVmFactory.Create(this, k))
            );
            UpdateKeyframes(PropertyTimelineViewModel.LayerPropertiesViewModel.PixelsPerSecond);
        }

        public void UpdateKeyframes(int pixelsPerSecond)
        {
            foreach (var keyframeViewModel in KeyframeViewModels)
            {
                keyframeViewModel.ParentView = View;
                keyframeViewModel.Update(pixelsPerSecond);
            }
        }

        protected override void OnViewLoaded()
        {
            foreach (var keyframeViewModel in KeyframeViewModels)
                keyframeViewModel.ParentView = View;
            base.OnViewLoaded();
        }
    }
}
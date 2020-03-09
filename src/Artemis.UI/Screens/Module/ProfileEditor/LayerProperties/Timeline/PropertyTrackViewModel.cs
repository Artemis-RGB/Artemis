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
        }


        public PropertyTimelineViewModel PropertyTimelineViewModel { get; }
        public LayerPropertyViewModel LayerPropertyViewModel { get; }
        public BindableCollection<PropertyTrackKeyframeViewModel> KeyframeViewModels { get; set; }

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
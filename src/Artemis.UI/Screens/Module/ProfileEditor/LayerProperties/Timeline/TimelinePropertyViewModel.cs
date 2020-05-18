using System.Linq;
using Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.Abstract;
using Artemis.UI.Services.Interfaces;
using Stylet;

namespace Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.Timeline
{
    public class TimelinePropertyViewModel<T> : TimelinePropertyViewModel
    {
        private readonly IProfileEditorService _profileEditorService;

        public TimelinePropertyViewModel(LayerPropertyBaseViewModel layerPropertyBaseViewModel, IProfileEditorService profileEditorService) : base(layerPropertyBaseViewModel)
        {
            _profileEditorService = profileEditorService;
            LayerPropertyViewModel = (LayerPropertyViewModel<T>) layerPropertyBaseViewModel;
        }

        public LayerPropertyViewModel<T> LayerPropertyViewModel { get; }

        public override void UpdateKeyframes(TimelineViewModel timelineViewModel)
        {
            var keyframes = LayerPropertyViewModel.LayerProperty.Keyframes.ToList();
            TimelineKeyframeViewModels.RemoveRange(
                TimelineKeyframeViewModels.Where(t => !keyframes.Contains(t.BaseLayerPropertyKeyframe))
            );
            TimelineKeyframeViewModels.AddRange(
                keyframes.Where(k => TimelineKeyframeViewModels.All(t => t.BaseLayerPropertyKeyframe != k))
                    .Select(k => new TimelineKeyframeViewModel<T>(_profileEditorService, timelineViewModel, k))
            );

            foreach (var timelineKeyframeViewModel in TimelineKeyframeViewModels)
                timelineKeyframeViewModel.Update(_profileEditorService.PixelsPerSecond);
        }
    }

    public abstract class TimelinePropertyViewModel
    {
        protected TimelinePropertyViewModel(LayerPropertyBaseViewModel layerPropertyBaseViewModel)
        {
            LayerPropertyBaseViewModel = layerPropertyBaseViewModel;
            TimelineKeyframeViewModels = new BindableCollection<TimelineKeyframeViewModel>();
        }

        public LayerPropertyBaseViewModel LayerPropertyBaseViewModel { get; }
        public BindableCollection<TimelineKeyframeViewModel> TimelineKeyframeViewModels { get; set; }

        public abstract void UpdateKeyframes(TimelineViewModel timelineViewModel);
    }
}
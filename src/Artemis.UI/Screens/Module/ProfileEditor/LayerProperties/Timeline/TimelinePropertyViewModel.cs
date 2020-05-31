using System;
using System.Linq;
using Artemis.UI.Exceptions;
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

            LayerPropertyViewModel.LayerProperty.KeyframeAdded += LayerPropertyOnKeyframeModified;
            LayerPropertyViewModel.LayerProperty.KeyframeRemoved += LayerPropertyOnKeyframeModified;
            LayerPropertyViewModel.LayerProperty.KeyframesToggled += LayerPropertyOnKeyframeModified;
            _profileEditorService.PixelsPerSecondChanged += ProfileEditorServiceOnPixelsPerSecondChanged;
        }

        private void LayerPropertyOnKeyframeModified(object sender, EventArgs e)
        {
            UpdateKeyframes();
        }

        private void ProfileEditorServiceOnPixelsPerSecondChanged(object sender, EventArgs e)
        {
            foreach (var timelineKeyframeViewModel in TimelineKeyframeViewModels)
                timelineKeyframeViewModel.Update(_profileEditorService.PixelsPerSecond);
        }

        public LayerPropertyViewModel<T> LayerPropertyViewModel { get; }

        public override void UpdateKeyframes()
        {
            if (TimelineViewModel == null)
                throw new ArtemisUIException("Timeline view model must be set before keyframes can be updated");

            // Only show keyframes if they are enabled
            if (LayerPropertyViewModel.LayerProperty.KeyframesEnabled)
            {
                var keyframes = LayerPropertyViewModel.LayerProperty.Keyframes.ToList();
                var toRemove = TimelineKeyframeViewModels.Where(t => !keyframes.Contains(t.BaseLayerPropertyKeyframe)).ToList();
                TimelineKeyframeViewModels.RemoveRange(toRemove);
                TimelineKeyframeViewModels.AddRange(
                    keyframes.Where(k => TimelineKeyframeViewModels.All(t => t.BaseLayerPropertyKeyframe != k))
                        .Select(k => new TimelineKeyframeViewModel<T>(_profileEditorService, TimelineViewModel, k))
                );
            }
            else
            {
                TimelineKeyframeViewModels.Clear();
            }

            foreach (var timelineKeyframeViewModel in TimelineKeyframeViewModels)
                timelineKeyframeViewModel.Update(_profileEditorService.PixelsPerSecond);
        }

        public override void Dispose()
        {
            _profileEditorService.PixelsPerSecondChanged -= ProfileEditorServiceOnPixelsPerSecondChanged;
            LayerPropertyViewModel.LayerProperty.KeyframeAdded -= LayerPropertyOnKeyframeModified;
            LayerPropertyViewModel.LayerProperty.KeyframeRemoved -= LayerPropertyOnKeyframeModified;
            LayerPropertyViewModel.LayerProperty.KeyframesToggled -= LayerPropertyOnKeyframeModified;
        }
    }

    public abstract class TimelinePropertyViewModel : IDisposable
    {
        protected TimelinePropertyViewModel(LayerPropertyBaseViewModel layerPropertyBaseViewModel)
        {
            LayerPropertyBaseViewModel = layerPropertyBaseViewModel;
            TimelineKeyframeViewModels = new BindableCollection<TimelineKeyframeViewModel>();
        }

        public LayerPropertyBaseViewModel LayerPropertyBaseViewModel { get; }
        public TimelineViewModel TimelineViewModel { get; set; }
        public BindableCollection<TimelineKeyframeViewModel> TimelineKeyframeViewModels { get; set; }

        public abstract void UpdateKeyframes();

        public abstract void Dispose();
    }
}
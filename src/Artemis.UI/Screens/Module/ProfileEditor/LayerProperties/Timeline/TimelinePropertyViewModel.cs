using System;
using System.Linq;
using Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.Abstract;
using Artemis.UI.Shared.Services.Interfaces;
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

        public LayerPropertyViewModel<T> LayerPropertyViewModel { get; }

        public override void UpdateKeyframes()
        {
            // Only show keyframes if they are enabled
            if (LayerPropertyViewModel.LayerProperty.KeyframesEnabled)
            {
                var keyframes = LayerPropertyViewModel.LayerProperty.Keyframes.ToList();
                var toRemove = TimelineKeyframeViewModels.Where(t => !keyframes.Contains(t.BaseLayerPropertyKeyframe)).ToList();
                TimelineKeyframeViewModels.RemoveRange(toRemove);
                TimelineKeyframeViewModels.AddRange(
                    keyframes.Where(k => TimelineKeyframeViewModels.All(t => t.BaseLayerPropertyKeyframe != k))
                        .Select(k => new TimelineKeyframeViewModel<T>(_profileEditorService, k))
                );
            }
            else
                TimelineKeyframeViewModels.Clear();

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

        private void LayerPropertyOnKeyframeModified(object sender, EventArgs e)
        {
            UpdateKeyframes();
        }

        private void ProfileEditorServiceOnPixelsPerSecondChanged(object sender, EventArgs e)
        {
            foreach (var timelineKeyframeViewModel in TimelineKeyframeViewModels)
                timelineKeyframeViewModel.Update(_profileEditorService.PixelsPerSecond);
        }
    }

    public abstract class TimelinePropertyViewModel : PropertyChangedBase, IDisposable
    {
        private BindableCollection<TimelineKeyframeViewModel> _timelineKeyframeViewModels;

        protected TimelinePropertyViewModel(LayerPropertyBaseViewModel layerPropertyBaseViewModel)
        {
            LayerPropertyBaseViewModel = layerPropertyBaseViewModel;
            TimelineKeyframeViewModels = new BindableCollection<TimelineKeyframeViewModel>();
        }

        public LayerPropertyBaseViewModel LayerPropertyBaseViewModel { get; }

        public BindableCollection<TimelineKeyframeViewModel> TimelineKeyframeViewModels
        {
            get => _timelineKeyframeViewModels;
            set => SetAndNotify(ref _timelineKeyframeViewModels, value);
        }

        public abstract void Dispose();

        public abstract void UpdateKeyframes();
    }
}
using System;
using System.Linq;
using Artemis.UI.Screens.ProfileEditor.LayerProperties.Abstract;
using Artemis.UI.Shared.Services;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.LayerProperties.Timeline
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
                foreach (var timelineKeyframeViewModel in toRemove)
                    timelineKeyframeViewModel.Dispose();

                TimelineKeyframeViewModels.RemoveRange(toRemove);
                TimelineKeyframeViewModels.AddRange(keyframes
                    .Where(k => TimelineKeyframeViewModels.All(t => t.BaseLayerPropertyKeyframe != k))
                    .Select(k => new TimelineKeyframeViewModel<T>(_profileEditorService, k))
                );
            }
            else
                DisposeKeyframeViewModels();

            foreach (var timelineKeyframeViewModel in TimelineKeyframeViewModels)
                timelineKeyframeViewModel.Update(_profileEditorService.PixelsPerSecond);
        }

        public override void Dispose()
        {
            _profileEditorService.PixelsPerSecondChanged -= ProfileEditorServiceOnPixelsPerSecondChanged;
            LayerPropertyViewModel.LayerProperty.KeyframeAdded -= LayerPropertyOnKeyframeModified;
            LayerPropertyViewModel.LayerProperty.KeyframeRemoved -= LayerPropertyOnKeyframeModified;
            LayerPropertyViewModel.LayerProperty.KeyframesToggled -= LayerPropertyOnKeyframeModified;
            DisposeKeyframeViewModels();
        }

        private void DisposeKeyframeViewModels()
        {
            foreach (var timelineKeyframeViewModel in TimelineKeyframeViewModels)
                timelineKeyframeViewModel.Dispose();
            TimelineKeyframeViewModels.Clear();
        }

        private void LayerPropertyOnKeyframeModified(object sender, EventArgs e)
        {
            UpdateKeyframes();
        }

        private void ProfileEditorServiceOnPixelsPerSecondChanged(object sender, EventArgs e)
        {
            foreach (var timelineKeyframeViewModel in TimelineKeyframeViewModels)
                timelineKeyframeViewModel.Update(_profileEditorService.PixelsPerSecond);

            Width = TimelineKeyframeViewModels.Any() ? TimelineKeyframeViewModels.Max(t => t.X) + 25 : 0;
        }
    }

    public abstract class TimelinePropertyViewModel : PropertyChangedBase, IDisposable
    {
        private BindableCollection<TimelineKeyframeViewModel> _timelineKeyframeViewModels;
        private double _width;

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

        public double Width
        {
            get => _width;
            set => SetAndNotify(ref _width, value);
        }

        public abstract void Dispose();

        public abstract void UpdateKeyframes();
    }
}
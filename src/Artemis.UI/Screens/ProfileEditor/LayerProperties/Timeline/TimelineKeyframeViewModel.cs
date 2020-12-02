using System;
using System.ComponentModel;
using System.Linq;
using Artemis.Core;
using Artemis.UI.Shared.Services;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.LayerProperties.Timeline
{
    public sealed class TimelineKeyframeViewModel<T> : Screen, ITimelineKeyframeViewModel
    {
        private readonly IProfileEditorService _profileEditorService;

        private bool _isSelected;
        private string _timestamp;
        private double _x;

        public TimelineKeyframeViewModel(LayerPropertyKeyframe<T> layerPropertyKeyframe, IProfileEditorService profileEditorService)
        {
            _profileEditorService = profileEditorService;
            LayerPropertyKeyframe = layerPropertyKeyframe;
            EasingViewModels = new BindableCollection<TimelineEasingViewModel>();

            _profileEditorService.PixelsPerSecondChanged += ProfileEditorServiceOnPixelsPerSecondChanged;
            LayerPropertyKeyframe.PropertyChanged += LayerPropertyKeyframeOnPropertyChanged;
        }


        public LayerPropertyKeyframe<T> LayerPropertyKeyframe { get; }
        public BindableCollection<TimelineEasingViewModel> EasingViewModels { get; }

        public double X
        {
            get => _x;
            set => SetAndNotify(ref _x, value);
        }

        public string Timestamp
        {
            get => _timestamp;
            set => SetAndNotify(ref _timestamp, value);
        }

        public bool IsSelected
        {
            get => _isSelected;
            set => SetAndNotify(ref _isSelected, value);
        }

        public TimeSpan Position => LayerPropertyKeyframe.Position;
        public ILayerPropertyKeyframe Keyframe => LayerPropertyKeyframe;

        public void Dispose()
        {
            _profileEditorService.PixelsPerSecondChanged -= ProfileEditorServiceOnPixelsPerSecondChanged;
            LayerPropertyKeyframe.PropertyChanged -= LayerPropertyKeyframeOnPropertyChanged;

            foreach (TimelineEasingViewModel timelineEasingViewModel in EasingViewModels)
                timelineEasingViewModel.EasingModeSelected -= TimelineEasingViewModelOnEasingModeSelected;
        }

        public void Update()
        {
            X = _profileEditorService.PixelsPerSecond * LayerPropertyKeyframe.Position.TotalSeconds;
            Timestamp = $"{Math.Floor(LayerPropertyKeyframe.Position.TotalSeconds):00}.{LayerPropertyKeyframe.Position.Milliseconds:000}";
        }

        private void ProfileEditorServiceOnPixelsPerSecondChanged(object sender, EventArgs e)
        {
            Update();
        }

        private void LayerPropertyKeyframeOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(LayerPropertyKeyframe.Position))
                Update();
        }

        #region Easing

        public void PopulateEasingViewModels()
        {
            if (EasingViewModels.Any())
                return;

            EasingViewModels.AddRange(Enum.GetValues(typeof(Easings.Functions))
                .Cast<Easings.Functions>()
                .Select(e => new TimelineEasingViewModel(e, e == LayerPropertyKeyframe.EasingFunction)));

            foreach (TimelineEasingViewModel timelineEasingViewModel in EasingViewModels)
                timelineEasingViewModel.EasingModeSelected += TimelineEasingViewModelOnEasingModeSelected;
        }

        public void ClearEasingViewModels()
        {
            EasingViewModels.Clear();
        }

        private void TimelineEasingViewModelOnEasingModeSelected(object sender, EventArgs e)
        {
            SelectEasingMode((TimelineEasingViewModel) sender);
        }

        public void SelectEasingMode(TimelineEasingViewModel easingViewModel)
        {
            LayerPropertyKeyframe.EasingFunction = easingViewModel.EasingFunction;
            // Set every selection to false except on the VM that made the change
            foreach (TimelineEasingViewModel propertyTrackEasingViewModel in EasingViewModels.Where(vm => vm != easingViewModel))
                propertyTrackEasingViewModel.IsEasingModeSelected = false;

            _profileEditorService.UpdateSelectedProfileElement();
        }

        #endregion

        #region Movement

        private TimeSpan? _offset;

        public void ReleaseMovement()
        {
            _offset = null;
        }

        public void SaveOffsetToKeyframe(ITimelineKeyframeViewModel source)
        {
            if (source == this)
            {
                _offset = null;
                return;
            }

            if (_offset != null)
                return;

            _offset = LayerPropertyKeyframe.Position - source.Position;
        }

        public void ApplyOffsetToKeyframe(ITimelineKeyframeViewModel source)
        {
            if (source == this || _offset == null)
                return;

            UpdatePosition(source.Position + _offset.Value);
        }

        public void UpdatePosition(TimeSpan position)
        {
            if (position < TimeSpan.Zero)
                LayerPropertyKeyframe.Position = TimeSpan.Zero;
            else if (position > _profileEditorService.SelectedProfileElement.Timeline.Length)
                LayerPropertyKeyframe.Position = _profileEditorService.SelectedProfileElement.Timeline.Length;
            else
                LayerPropertyKeyframe.Position = position;

            Update();
        }

        #endregion

        #region Context menu actions
        
        public void Delete(bool save = true)
        {
            LayerPropertyKeyframe.LayerProperty.RemoveKeyframe(LayerPropertyKeyframe);
            if (save)
                _profileEditorService.UpdateSelectedProfileElement();
        }

        #endregion
    }

    public interface ITimelineKeyframeViewModel : IScreen, IDisposable
    {
        bool IsSelected { get; set; }
        TimeSpan Position { get; }
        ILayerPropertyKeyframe Keyframe { get; }

        #region Movement

        void SaveOffsetToKeyframe(ITimelineKeyframeViewModel source);
        void ApplyOffsetToKeyframe(ITimelineKeyframeViewModel source);
        void UpdatePosition(TimeSpan position);
        void ReleaseMovement();

        #endregion

        #region Context menu actions

        void PopulateEasingViewModels();
        void ClearEasingViewModels();
        void Delete(bool save = true);

        #endregion
    }
}
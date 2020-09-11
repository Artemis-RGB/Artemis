using System;
using System.ComponentModel;
using System.Linq;
using Artemis.Core;
using Artemis.UI.Shared.Services;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.LayerProperties.Timeline
{
    public class TimelineKeyframeViewModel<T> : Screen, ITimelineKeyframeViewModel
    {
        private readonly IProfileEditorService _profileEditorService;

        private BindableCollection<TimelineEasingViewModel> _easingViewModels;
        private bool _isSelected;
        private string _timestamp;
        private double _x;

        public TimelineKeyframeViewModel(LayerPropertyKeyframe<T> layerPropertyKeyframe, IProfileEditorService profileEditorService)
        {
            _profileEditorService = profileEditorService;
            LayerPropertyKeyframe = layerPropertyKeyframe;
            LayerPropertyKeyframe.PropertyChanged += LayerPropertyKeyframeOnPropertyChanged;
        }

        public LayerPropertyKeyframe<T> LayerPropertyKeyframe { get; }

        public BindableCollection<TimelineEasingViewModel> EasingViewModels
        {
            get => _easingViewModels;
            set => SetAndNotify(ref _easingViewModels, value);
        }

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

        public void Dispose()
        {
            LayerPropertyKeyframe.PropertyChanged -= LayerPropertyKeyframeOnPropertyChanged;

            foreach (var timelineEasingViewModel in EasingViewModels)
                timelineEasingViewModel.EasingModeSelected -= TimelineEasingViewModelOnEasingModeSelected;
        }

        public void Update()
        {
            X = _profileEditorService.PixelsPerSecond * LayerPropertyKeyframe.Position.TotalSeconds;
            Timestamp = $"{Math.Floor(LayerPropertyKeyframe.Position.TotalSeconds):00}.{LayerPropertyKeyframe.Position.Milliseconds:000}";
        }

        private void LayerPropertyKeyframeOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(LayerPropertyKeyframe.Position))
                Update();
        }

        #region Easing

        public void CreateEasingViewModels()
        {
            if (EasingViewModels.Any())
                return;

            EasingViewModels.AddRange(Enum.GetValues(typeof(Easings.Functions))
                .Cast<Easings.Functions>()
                .Select(e => new TimelineEasingViewModel(e, e == LayerPropertyKeyframe.EasingFunction)));

            foreach (var timelineEasingViewModel in EasingViewModels)
                timelineEasingViewModel.EasingModeSelected += TimelineEasingViewModelOnEasingModeSelected;
        }

        private void TimelineEasingViewModelOnEasingModeSelected(object sender, EventArgs e)
        {
            SelectEasingMode((TimelineEasingViewModel) sender);
        }

        public void SelectEasingMode(TimelineEasingViewModel easingViewModel)
        {
            LayerPropertyKeyframe.EasingFunction = easingViewModel.EasingFunction;
            // Set every selection to false except on the VM that made the change
            foreach (var propertyTrackEasingViewModel in EasingViewModels.Where(vm => vm != easingViewModel))
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
            else if (position > _profileEditorService.SelectedProfileElement.TimelineLength)
                LayerPropertyKeyframe.Position = _profileEditorService.SelectedProfileElement.TimelineLength;
            else
                LayerPropertyKeyframe.Position = position;

            Update();
        }

        #endregion

        #region Context menu actions

        public void ContextMenuOpening()
        {
            CreateEasingViewModels();
        }

        public void ContextMenuClosing()
        {
            foreach (var timelineEasingViewModel in EasingViewModels)
                timelineEasingViewModel.EasingModeSelected -= TimelineEasingViewModelOnEasingModeSelected;
            EasingViewModels.Clear();
        }

        public void Copy()
        {
            var newKeyframe = new LayerPropertyKeyframe<T>(
                LayerPropertyKeyframe.Value,
                LayerPropertyKeyframe.Position,
                LayerPropertyKeyframe.EasingFunction,
                LayerPropertyKeyframe.LayerProperty
            );
            // If possible, shift the keyframe to the right by 11 pixels
            var desiredPosition = newKeyframe.Position + TimeSpan.FromMilliseconds(1000f / _profileEditorService.PixelsPerSecond * 11);
            if (desiredPosition <= newKeyframe.LayerProperty.ProfileElement.TimelineLength)
                newKeyframe.Position = desiredPosition;
            // Otherwise if possible shift it to the left by 11 pixels
            else
            {
                desiredPosition = newKeyframe.Position - TimeSpan.FromMilliseconds(1000f / _profileEditorService.PixelsPerSecond * 11);
                if (desiredPosition > TimeSpan.Zero)
                    newKeyframe.Position = desiredPosition;
            }

            LayerPropertyKeyframe.LayerProperty.AddKeyframe(newKeyframe);
            _profileEditorService.UpdateSelectedProfileElement();
        }

        public void Delete()
        {
            LayerPropertyKeyframe.LayerProperty.RemoveKeyframe(LayerPropertyKeyframe);
            _profileEditorService.UpdateSelectedProfileElement();
        }

        #endregion
    }

    public interface ITimelineKeyframeViewModel : IScreen, IDisposable
    {
        bool IsSelected { get; set; }
        TimeSpan Position { get; }

        #region Movement

        void SaveOffsetToKeyframe(ITimelineKeyframeViewModel source);
        void ApplyOffsetToKeyframe(ITimelineKeyframeViewModel source);
        void UpdatePosition(TimeSpan position);
        void ReleaseMovement();

        #endregion

        #region Context menu actions

        void Copy();
        void Delete();

        #endregion
    }
}
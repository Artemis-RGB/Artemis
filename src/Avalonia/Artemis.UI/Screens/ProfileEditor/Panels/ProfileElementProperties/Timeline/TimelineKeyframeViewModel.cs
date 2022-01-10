using System;
using System.Collections.ObjectModel;
using System.Linq;
using Artemis.Core;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services.ProfileEditor;
using Avalonia.Controls.Mixins;
using DynamicData;
using ReactiveUI;
using Disposable = System.Reactive.Disposables.Disposable;

namespace Artemis.UI.Screens.ProfileEditor.ProfileElementProperties.Timeline
{
    public class TimelineKeyframeViewModel<T> : ActivatableViewModelBase, ITimelineKeyframeViewModel
    {

        private bool _isSelected;
        private string _timestamp;
        private double _x;
        private readonly IProfileEditorService _profileEditorService;

        public TimelineKeyframeViewModel(LayerPropertyKeyframe<T> layerPropertyKeyframe, IProfileEditorService profileEditorService)
        {
            _profileEditorService = profileEditorService;
            LayerPropertyKeyframe = layerPropertyKeyframe;
            EasingViewModels = new ObservableCollection<TimelineEasingViewModel>();

            this.WhenActivated(d =>
            {
                _profileEditorService.PixelsPerSecond.Subscribe(p =>
                {
                    _pixelsPerSecond = p;
                    _profileEditorService.PixelsPerSecond.Subscribe(_ => Update()).DisposeWith(d);
                    Disposable.Create(() =>
                    {
                        foreach (TimelineEasingViewModel timelineEasingViewModel in EasingViewModels)
                            timelineEasingViewModel.EasingModeSelected -= TimelineEasingViewModelOnEasingModeSelected;
                    }).DisposeWith(d);
                }).DisposeWith(d);
            });
        }

        public LayerPropertyKeyframe<T> LayerPropertyKeyframe { get; }
        public ObservableCollection<TimelineEasingViewModel> EasingViewModels { get; }

        public double X
        {
            get => _x;
            set => this.RaiseAndSetIfChanged(ref _x, value);
        }

        public string Timestamp
        {
            get => _timestamp;
            set => this.RaiseAndSetIfChanged(ref _timestamp, value);
        }

        public bool IsSelected
        {
            get => _isSelected;
            set => this.RaiseAndSetIfChanged(ref _isSelected, value);
        }

        public TimeSpan Position => LayerPropertyKeyframe.Position;
        public ILayerPropertyKeyframe Keyframe => LayerPropertyKeyframe;

        public void Update()
        {
            X = _pixelsPerSecond * LayerPropertyKeyframe.Position.TotalSeconds;
            Timestamp = $"{Math.Floor(LayerPropertyKeyframe.Position.TotalSeconds):00}.{LayerPropertyKeyframe.Position.Milliseconds:000}";
        }


        #region Movement

        private TimeSpan? _offset;
        private double _pixelsPerSecond;

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
            throw new NotImplementedException();

            // if (position < TimeSpan.Zero)
            //     LayerPropertyKeyframe.Position = TimeSpan.Zero;
            // else if (position > _profileEditorService.SelectedProfileElement.Timeline.Length)
            //     LayerPropertyKeyframe.Position = _profileEditorService.SelectedProfileElement.Timeline.Length;
            // else
            //     LayerPropertyKeyframe.Position = position;

            Update();
        }

        #endregion

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

        private void TimelineEasingViewModelOnEasingModeSelected(object? sender, EventArgs e)
        {
            if (sender is TimelineEasingViewModel timelineEasingViewModel)
                SelectEasingMode(timelineEasingViewModel);
        }

        public void SelectEasingMode(TimelineEasingViewModel easingViewModel)
        {
            throw new NotImplementedException();

            LayerPropertyKeyframe.EasingFunction = easingViewModel.EasingFunction;
            // Set every selection to false except on the VM that made the change
            foreach (TimelineEasingViewModel propertyTrackEasingViewModel in EasingViewModels.Where(vm => vm != easingViewModel))
                propertyTrackEasingViewModel.IsEasingModeSelected = false;
        }

        #endregion

        #region Context menu actions

        public void Delete(bool save = true)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}

using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Artemis.Core.Models.Profile.LayerProperties;
using Artemis.Core.Utilities;
using Artemis.UI.Services.Interfaces;
using Stylet;

namespace Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.Timeline
{
    public class TimelineKeyframeViewModel<T> : TimelineKeyframeViewModel
    {
        private readonly IProfileEditorService _profileEditorService;

        public TimelineKeyframeViewModel(IProfileEditorService profileEditorService, TimelineViewModel timelineViewModel, LayerPropertyKeyframe<T> layerPropertyKeyframe)
            : base(profileEditorService, timelineViewModel, layerPropertyKeyframe)
        {
            _profileEditorService = profileEditorService;
            LayerPropertyKeyframe = layerPropertyKeyframe;
        }

        public LayerPropertyKeyframe<T> LayerPropertyKeyframe { get; }

        #region Context menu actions

        public void Copy()
        {
            var newKeyframe = new LayerPropertyKeyframe<T>(
                LayerPropertyKeyframe.Value,
                LayerPropertyKeyframe.Position,
                LayerPropertyKeyframe.EasingFunction,
                LayerPropertyKeyframe.LayerProperty
            );
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

    public abstract class TimelineKeyframeViewModel : PropertyChangedBase
    {
        private readonly IProfileEditorService _profileEditorService;
        private readonly TimelineViewModel _timelineViewModel;
        private int _pixelsPerSecond;

        protected TimelineKeyframeViewModel(IProfileEditorService profileEditorService, TimelineViewModel timelineViewModel, BaseLayerPropertyKeyframe baseLayerPropertyKeyframe)
        {
            _profileEditorService = profileEditorService;
            _timelineViewModel = timelineViewModel;
            BaseLayerPropertyKeyframe = baseLayerPropertyKeyframe;
            EasingViewModels = new BindableCollection<TimelineEasingViewModel>();
        }

        public BaseLayerPropertyKeyframe BaseLayerPropertyKeyframe { get; }
        public BindableCollection<TimelineEasingViewModel> EasingViewModels { get; set; }

        public bool IsSelected { get; set; }
        public double X { get; set; }
        public string Timestamp { get; set; }

        public UIElement ParentView { get; set; }

        public void Update(int pixelsPerSecond)
        {
            _pixelsPerSecond = pixelsPerSecond;

            X = pixelsPerSecond * BaseLayerPropertyKeyframe.Position.TotalSeconds;
            Timestamp = $"{Math.Floor(BaseLayerPropertyKeyframe.Position.TotalSeconds):00}.{BaseLayerPropertyKeyframe.Position.Milliseconds:000}";
        }

        #region Keyframe movement

        public void KeyframeMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Released)
                return;

            ((IInputElement) sender).CaptureMouse();
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift) && !IsSelected)
                _timelineViewModel.SelectKeyframe(this, true, false);
            else if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                _timelineViewModel.SelectKeyframe(this, false, true);
            else if (!IsSelected)
                _timelineViewModel.SelectKeyframe(this, false, false);

            e.Handled = true;
        }

        public void KeyframeMouseUp(object sender, MouseButtonEventArgs e)
        {
            _profileEditorService.UpdateSelectedProfileElement();
            _timelineViewModel.ReleaseSelectedKeyframes();

            ((IInputElement) sender).ReleaseMouseCapture();
        }

        public void KeyframeMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                _timelineViewModel.MoveSelectedKeyframes(GetCursorTime(e.GetPosition(ParentView)));

            e.Handled = true;
        }

        private TimeSpan GetCursorTime(Point position)
        {
            // Get the parent grid, need that for our position
            var x = Math.Max(0, position.X);
            var time = TimeSpan.FromSeconds(x / _pixelsPerSecond);

            // Round the time to something that fits the current zoom level
            if (_pixelsPerSecond < 200)
                time = TimeSpan.FromMilliseconds(Math.Round(time.TotalMilliseconds / 5.0) * 5.0);
            else if (_pixelsPerSecond < 500)
                time = TimeSpan.FromMilliseconds(Math.Round(time.TotalMilliseconds / 2.0) * 2.0);
            else
                time = TimeSpan.FromMilliseconds(Math.Round(time.TotalMilliseconds));

            // If shift is held, snap to the current time
            // Take a tolerance of 5 pixels (half a keyframe width)
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                var tolerance = 1000f / _pixelsPerSecond * 5;
                if (Math.Abs(_profileEditorService.CurrentTime.TotalMilliseconds - time.TotalMilliseconds) < tolerance)
                    time = _profileEditorService.CurrentTime;
            }

            return time;
        }

        #endregion

        #region Easing

        public void ContextMenuOpening()
        {
            CreateEasingViewModels();
        }

        public void ContextMenuClosing()
        {
            EasingViewModels.Clear();
        }

        private void CreateEasingViewModels()
        {
            EasingViewModels.AddRange(Enum.GetValues(typeof(Easings.Functions)).Cast<Easings.Functions>().Select(v => new TimelineEasingViewModel(this, v)));
        }

        public void SelectEasingMode(TimelineEasingViewModel easingViewModel)
        {
            BaseLayerPropertyKeyframe.EasingFunction = easingViewModel.EasingFunction;
            // Set every selection to false except on the VM that made the change
            foreach (var propertyTrackEasingViewModel in EasingViewModels.Where(vm => vm != easingViewModel))
                propertyTrackEasingViewModel.IsEasingModeSelected = false;


            _profileEditorService.UpdateSelectedProfileElement();
        }

        #endregion

        #region Movement

        private bool _movementReleased = true;
        private TimeSpan _startOffset;

        public void ApplyMovement(TimeSpan cursorTime)
        {
            if (_movementReleased)
            {
                _movementReleased = false;
                _startOffset = cursorTime - BaseLayerPropertyKeyframe.Position;
            }
            else
            {
                BaseLayerPropertyKeyframe.Position = cursorTime - _startOffset;
                if (BaseLayerPropertyKeyframe.Position < TimeSpan.Zero)
                    BaseLayerPropertyKeyframe.Position = TimeSpan.Zero;

                Update(_pixelsPerSecond);
            }
        }

        public void ReleaseMovement()
        {
            _movementReleased = true;
        }

        #endregion
    }
}
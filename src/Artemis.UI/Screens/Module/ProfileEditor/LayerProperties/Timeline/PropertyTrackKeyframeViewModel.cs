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
    public class PropertyTrackKeyframeViewModel : PropertyChangedBase
    {
        private readonly IProfileEditorService _profileEditorService;
        private int _pixelsPerSecond;

        public PropertyTrackKeyframeViewModel(PropertyTrackViewModel propertyTrackViewModel, BaseKeyframe keyframe, IProfileEditorService profileEditorService)
        {
            _profileEditorService = profileEditorService;

            PropertyTrackViewModel = propertyTrackViewModel;
            Keyframe = keyframe;
            EasingViewModels = new BindableCollection<PropertyTrackEasingViewModel>();
            CreateEasingViewModels();
        }

        public bool IsSelected { get; set; }
        public PropertyTrackViewModel PropertyTrackViewModel { get; }
        public BaseKeyframe Keyframe { get; }
        public BindableCollection<PropertyTrackEasingViewModel> EasingViewModels { get; set; }
        public double X { get; set; }
        public string Timestamp { get; set; }

        public UIElement ParentView { get; set; }


        public void Update(int pixelsPerSecond)
        {
            _pixelsPerSecond = pixelsPerSecond;

            X = pixelsPerSecond * Keyframe.Position.TotalSeconds;
            Timestamp = $"{Math.Floor(Keyframe.Position.TotalSeconds):00}.{Keyframe.Position.Milliseconds:000}";
        }

        #region Keyframe movement

        public void KeyframeMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Released)
                return;

            ((IInputElement) sender).CaptureMouse();
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift) && !IsSelected)
                PropertyTrackViewModel.PropertyTimelineViewModel.SelectKeyframe(this, true, false);
            else if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                PropertyTrackViewModel.PropertyTimelineViewModel.SelectKeyframe(this, false, true);
            else if (!IsSelected)
                PropertyTrackViewModel.PropertyTimelineViewModel.SelectKeyframe(this, false, false);

            e.Handled = true;
        }

        public void KeyframeMouseUp(object sender, MouseButtonEventArgs e)
        {
            _profileEditorService.UpdateSelectedProfileElement();
            PropertyTrackViewModel.PropertyTimelineViewModel.ReleaseSelectedKeyframes();

            ((IInputElement) sender).ReleaseMouseCapture();
        }

        public void KeyframeMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                PropertyTrackViewModel.PropertyTimelineViewModel.MoveSelectedKeyframes(GetCursorTime(e.GetPosition(ParentView)));

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

        #region Context menu actions

        public void Copy()
        {
            var keyframe = PropertyTrackViewModel.LayerPropertyViewModel.LayerProperty.CreateNewKeyframe(Keyframe.Position, Keyframe.BaseValue);
            keyframe.EasingFunction = Keyframe.EasingFunction;
            _profileEditorService.UpdateSelectedProfileElement();
        }

        public void Delete()
        {
            PropertyTrackViewModel.LayerPropertyViewModel.LayerProperty.RemoveKeyframe(Keyframe);
            _profileEditorService.UpdateSelectedProfileElement();
        }

        #endregion

        #region Easing

        private void CreateEasingViewModels()
        {
            foreach (Easings.Functions value in Enum.GetValues(typeof(Easings.Functions)))
                EasingViewModels.Add(new PropertyTrackEasingViewModel(this, value));
        }

        public void SelectEasingMode(PropertyTrackEasingViewModel easingViewModel)
        {
            Keyframe.EasingFunction = easingViewModel.EasingFunction;
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
                _startOffset = cursorTime - Keyframe.Position;
            }
            else
            {
                Keyframe.Position = cursorTime - _startOffset;
                if (Keyframe.Position < TimeSpan.Zero)
                    Keyframe.Position = TimeSpan.Zero;

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
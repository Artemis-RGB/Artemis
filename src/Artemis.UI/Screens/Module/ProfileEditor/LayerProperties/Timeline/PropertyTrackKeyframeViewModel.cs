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

        public PropertyTrackKeyframeViewModel(BaseKeyframe keyframe, IProfileEditorService profileEditorService)
        {
            _profileEditorService = profileEditorService;

            Keyframe = keyframe;
            EasingViewModels = new BindableCollection<PropertyTrackEasingViewModel>();
            CreateEasingViewModels();
        }

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
            ((IInputElement) sender).CaptureMouse();
        }

        public void KeyframeMouseUp(object sender, MouseButtonEventArgs e)
        {
            ((IInputElement) sender).ReleaseMouseCapture();
        }

        public void KeyframeMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                // Get the parent grid, need that for our position
                var x = Math.Max(0, e.GetPosition(ParentView).X);
                var newTime = TimeSpan.FromSeconds(x / _pixelsPerSecond);

                // Round the time to something that fits the current zoom level
                if (_pixelsPerSecond < 200)
                    newTime = TimeSpan.FromMilliseconds(Math.Round(newTime.TotalMilliseconds / 5.0) * 5.0);
                else if (_pixelsPerSecond < 500)
                    newTime = TimeSpan.FromMilliseconds(Math.Round(newTime.TotalMilliseconds / 2.0) * 2.0);
                else
                    newTime = TimeSpan.FromMilliseconds(Math.Round(newTime.TotalMilliseconds));

                if (!Keyboard.IsKeyDown(Key.LeftShift) && !Keyboard.IsKeyDown(Key.RightShift))
                {
                    Keyframe.Position = newTime;

                    Update(_pixelsPerSecond);
                    _profileEditorService.UpdateSelectedProfileElement();
                    return;
                }

                // If shift is held, snap to the current time
                // Take a tolerance of 5 pixels (half a keyframe width)
                var tolerance = 1000f / _pixelsPerSecond * 5;
                if (Math.Abs(_profileEditorService.CurrentTime.TotalMilliseconds - newTime.TotalMilliseconds) < tolerance)
                    Keyframe.Position = _profileEditorService.CurrentTime;
                else
                    Keyframe.Position = newTime;

                Update(_pixelsPerSecond);
                _profileEditorService.UpdateSelectedProfileElement();
            }
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
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Artemis.Core.Events;
using Artemis.Core.Models.Profile;
using Artemis.Core.Models.Profile.LayerProperties;
using Artemis.Core.Models.Profile.LayerProperties.Attributes;
using Artemis.Core.Services;
using Artemis.Core.Services.Interfaces;
using Artemis.UI.Events;
using Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.Timeline;
using Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.Tree;
using Artemis.UI.Services.Interfaces;
using Stylet;

namespace Artemis.UI.Screens.Module.ProfileEditor.LayerProperties
{
    public class LayerPropertiesViewModel : ProfileEditorPanelViewModel
    {
        private readonly ICoreService _coreService;
        private readonly IProfileEditorService _profileEditorService;
        private readonly ISettingsService _settingsService;

        public LayerPropertiesViewModel(IProfileEditorService profileEditorService, ICoreService coreService, ISettingsService settingsService)
        {
            _profileEditorService = profileEditorService;
            _coreService = coreService;
            _settingsService = settingsService;

            PixelsPerSecond = 31;
        }

        public bool Playing { get; set; }
        public bool RepeatAfterLastKeyframe { get; set; }
        public string FormattedCurrentTime => $"{Math.Floor(_profileEditorService.CurrentTime.TotalSeconds):00}.{_profileEditorService.CurrentTime.Milliseconds:000}";

        public int PixelsPerSecond
        {
            get => _pixelsPerSecond;
            set
            {
                _pixelsPerSecond = value;
                OnPixelsPerSecondChanged();
            }
        }

        public Thickness TimeCaretPosition
        {
            get => new Thickness(_profileEditorService.CurrentTime.TotalSeconds * PixelsPerSecond, 0, 0, 0);
            set => _profileEditorService.CurrentTime = TimeSpan.FromSeconds(value.Left / PixelsPerSecond);
        }

        public BindableCollection<LayerPropertyGroupViewModel> LayerPropertyGroups { get; set; }
        public TreeViewModel TreeViewModel { get; set; }
        public TimelineViewModel TimelineViewModel { get; set; }

        protected override void OnInitialActivate()
        {
            TreeViewModel = new TreeViewModel(LayerPropertyGroups);
            TimelineViewModel = new TimelineViewModel(LayerPropertyGroups);

            PopulateProperties(_profileEditorService.SelectedProfileElement);

            _profileEditorService.ProfileElementSelected += ProfileEditorServiceOnProfileElementSelected;
            _profileEditorService.CurrentTimeChanged += ProfileEditorServiceOnCurrentTimeChanged;

            base.OnInitialActivate();
        }

        protected override void OnClose()
        {
            _profileEditorService.ProfileElementSelected -= ProfileEditorServiceOnProfileElementSelected;
            _profileEditorService.CurrentTimeChanged -= ProfileEditorServiceOnCurrentTimeChanged;

            base.OnClose();
        }

        protected override void OnDeactivate()
        {
            Pause();
            base.OnDeactivate();
        }

        private void ProfileEditorServiceOnProfileElementSelected(object sender, ProfileElementEventArgs e)
        {
            PopulateProperties(e.ProfileElement);
        }

        private void ProfileEditorServiceOnCurrentTimeChanged(object sender, EventArgs e)
        {
            NotifyOfPropertyChange(() => FormattedCurrentTime);
            NotifyOfPropertyChange(() => TimeCaretPosition);
        }

        #region View model managament

        private void PopulateProperties(ProfileElement profileElement)
        {
            LayerPropertyGroups.Clear();
            if (profileElement is Layer layer)
            {
                // Add the built-in root groups of the layer
                var generalAttribute = Attribute.GetCustomAttribute(
                    layer.GetType().GetProperty(nameof(layer.General)),
                    typeof(PropertyGroupDescriptionAttribute)
                );
                var transformAttribute = Attribute.GetCustomAttribute(
                    layer.GetType().GetProperty(nameof(layer.Transform)),
                    typeof(PropertyGroupDescriptionAttribute)
                );
                LayerPropertyGroups.Add(new LayerPropertyGroupViewModel(_profileEditorService, layer.General, (PropertyGroupDescriptionAttribute) generalAttribute));
                LayerPropertyGroups.Add(new LayerPropertyGroupViewModel(_profileEditorService, layer.Transform, (PropertyGroupDescriptionAttribute) transformAttribute));

                // Add the rout group of the brush
                // The root group of the brush has no attribute so let's pull one out of our sleeve
                var brushDescription = new PropertyGroupDescriptionAttribute
                {
                    Name = layer.LayerBrush.Descriptor.DisplayName,
                    Description = layer.LayerBrush.Descriptor.Description
                };
                LayerPropertyGroups.Add(new LayerPropertyGroupViewModel(_profileEditorService, layer.LayerBrush.BaseProperties, brushDescription));
            }
        }

        #endregion

        #region Controls

        public void PlayFromStart()
        {
            if (!Playing)
                _profileEditorService.CurrentTime = TimeSpan.Zero;

            Play();
        }

        public void Play()
        {
            if (!IsActive)
                return;
            if (Playing)
            {
                Pause();
                return;
            }

            _coreService.FrameRendering += CoreServiceOnFrameRendering;
            Playing = true;
        }

        public void Pause()
        {
            if (!Playing)
                return;

            _coreService.FrameRendering -= CoreServiceOnFrameRendering;
            Playing = false;
        }


        public void GoToStart()
        {
            _profileEditorService.CurrentTime = TimeSpan.Zero;
        }

        public void GoToEnd()
        {
            _profileEditorService.CurrentTime = CalculateEndTime();
        }

        public void GoToPreviousFrame()
        {
            var frameTime = 1000.0 / _settingsService.GetSetting("Core.TargetFrameRate", 25).Value;
            var newTime = Math.Max(0, Math.Round((_profileEditorService.CurrentTime.TotalMilliseconds - frameTime) / frameTime) * frameTime);
            _profileEditorService.CurrentTime = TimeSpan.FromMilliseconds(newTime);
        }

        public void GoToNextFrame()
        {
            var frameTime = 1000.0 / _settingsService.GetSetting("Core.TargetFrameRate", 25).Value;
            var newTime = Math.Round((_profileEditorService.CurrentTime.TotalMilliseconds + frameTime) / frameTime) * frameTime;
            newTime = Math.Min(newTime, CalculateEndTime().TotalMilliseconds);
            _profileEditorService.CurrentTime = TimeSpan.FromMilliseconds(newTime);
        }

        private TimeSpan CalculateEndTime()
        {
            if (!(_profileEditorService.SelectedProfileElement is Layer layer))
                return TimeSpan.MaxValue;

            var keyframes = GetKeyframes(false);

            // If there are no keyframes, don't stop at all
            if (!keyframes.Any())
                return TimeSpan.MaxValue;
            // If there are keyframes, stop after the last keyframe + 10 sec
            return keyframes.Max(k => k.Position).Add(TimeSpan.FromSeconds(10));
        }

        private void CoreServiceOnFrameRendering(object sender, FrameRenderingEventArgs e)
        {
            Execute.PostToUIThread(() =>
            {
                var newTime = _profileEditorService.CurrentTime.Add(TimeSpan.FromSeconds(e.DeltaTime));
                if (RepeatAfterLastKeyframe)
                {
                    if (newTime > CalculateEndTime().Subtract(TimeSpan.FromSeconds(10)))
                        newTime = TimeSpan.Zero;
                }
                else if (newTime > CalculateEndTime())
                {
                    newTime = CalculateEndTime();
                    Pause();
                }

                _profileEditorService.CurrentTime = newTime;
            });
        }

        #endregion

        #region Caret movement

        private int _pixelsPerSecond;

        public void TimelineMouseDown(object sender, MouseButtonEventArgs e)
        {
            ((IInputElement) sender).CaptureMouse();
        }

        public void TimelineMouseUp(object sender, MouseButtonEventArgs e)
        {
            ((IInputElement) sender).ReleaseMouseCapture();
        }

        public void TimelineMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                // Get the parent grid, need that for our position
                var parent = (IInputElement) VisualTreeHelper.GetParent((DependencyObject) sender);
                var x = Math.Max(0, e.GetPosition(parent).X);
                var newTime = TimeSpan.FromSeconds(x / PixelsPerSecond);

                // Round the time to something that fits the current zoom level
                if (PixelsPerSecond < 200)
                    newTime = TimeSpan.FromMilliseconds(Math.Round(newTime.TotalMilliseconds / 5.0) * 5.0);
                else if (PixelsPerSecond < 500)
                    newTime = TimeSpan.FromMilliseconds(Math.Round(newTime.TotalMilliseconds / 2.0) * 2.0);
                else
                    newTime = TimeSpan.FromMilliseconds(Math.Round(newTime.TotalMilliseconds));

                if (!Keyboard.IsKeyDown(Key.LeftShift) && !Keyboard.IsKeyDown(Key.RightShift))
                {
                    _profileEditorService.CurrentTime = newTime;
                    return;
                }

                var visibleKeyframes = GetKeyframes(true);

                // Take a tolerance of 5 pixels (half a keyframe width)
                var tolerance = 1000f / PixelsPerSecond * 5;
                var closeKeyframe = visibleKeyframes.FirstOrDefault(k => Math.Abs(k.Position.TotalMilliseconds - newTime.TotalMilliseconds) < tolerance);
                _profileEditorService.CurrentTime = closeKeyframe?.Position ?? newTime;
            }
        }

        private List<BaseLayerPropertyKeyframe> GetKeyframes(bool visibleOnly)
        {
            var result = new List<BaseLayerPropertyKeyframe>();
            foreach (var layerPropertyGroupViewModel in LayerPropertyGroups)
                result.AddRange(layerPropertyGroupViewModel.GetKeyframes(visibleOnly));

            return result;
        }

        #endregion

        #region Events

        public event EventHandler PixelsPerSecondChanged;

        protected virtual void OnPixelsPerSecondChanged()
        {
            PixelsPerSecondChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}
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
        public LayerPropertiesViewModel(IProfileEditorService profileEditorService, ICoreService coreService, ISettingsService settingsService)
        {
            ProfileEditorService = profileEditorService;
            CoreService = coreService;
            SettingsService = settingsService;

            LayerPropertyGroups = new BindableCollection<LayerPropertyGroupViewModel>();
        }

        public IProfileEditorService ProfileEditorService { get; }
        public ICoreService CoreService { get; }
        public ISettingsService SettingsService { get; }

        public bool Playing { get; set; }
        public bool RepeatAfterLastKeyframe { get; set; }
        public string FormattedCurrentTime => $"{Math.Floor(ProfileEditorService.CurrentTime.TotalSeconds):00}.{ProfileEditorService.CurrentTime.Milliseconds:000}";

        public Thickness TimeCaretPosition
        {
            get => new Thickness(ProfileEditorService.CurrentTime.TotalSeconds * ProfileEditorService.PixelsPerSecond, 0, 0, 0);
            set => ProfileEditorService.CurrentTime = TimeSpan.FromSeconds(value.Left / ProfileEditorService.PixelsPerSecond);
        }

        public Layer SelectedLayer { get; set; }
        public BindableCollection<LayerPropertyGroupViewModel> LayerPropertyGroups { get; set; }
        public TreeViewModel TreeViewModel { get; set; }
        public TimelineViewModel TimelineViewModel { get; set; }

        protected override void OnInitialActivate()
        {
            PopulateProperties(ProfileEditorService.SelectedProfileElement);

            ProfileEditorService.ProfileElementSelected += ProfileEditorServiceOnProfileElementSelected;
            ProfileEditorService.CurrentTimeChanged += ProfileEditorServiceOnCurrentTimeChanged;
            ProfileEditorService.PixelsPerSecondChanged += ProfileEditorServiceOnPixelsPerSecondChanged;

            base.OnInitialActivate();
        }

        protected override void OnClose()
        {
            ProfileEditorService.ProfileElementSelected -= ProfileEditorServiceOnProfileElementSelected;
            ProfileEditorService.CurrentTimeChanged -= ProfileEditorServiceOnCurrentTimeChanged;
            ProfileEditorService.PixelsPerSecondChanged -= ProfileEditorServiceOnPixelsPerSecondChanged;

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

        private void ProfileEditorServiceOnPixelsPerSecondChanged(object? sender, EventArgs e)
        {
            NotifyOfPropertyChange(nameof(TimeCaretPosition));
        }

        #region View model managament

        private void PopulateProperties(ProfileElement profileElement)
        {
            if (SelectedLayer != null)
            {
                SelectedLayer.LayerBrushUpdated -= SelectedLayerOnLayerBrushUpdated;
                SelectedLayer = null;
            }

            foreach (var layerPropertyGroupViewModel in LayerPropertyGroups)
                layerPropertyGroupViewModel.Dispose();
            LayerPropertyGroups.Clear();

            if (profileElement is Layer layer)
            {
                SelectedLayer = layer;
                SelectedLayer.LayerBrushUpdated += SelectedLayerOnLayerBrushUpdated;

                // Add the built-in root groups of the layer
                var generalAttribute = Attribute.GetCustomAttribute(
                    layer.GetType().GetProperty(nameof(layer.General)),
                    typeof(PropertyGroupDescriptionAttribute)
                );
                var transformAttribute = Attribute.GetCustomAttribute(
                    layer.GetType().GetProperty(nameof(layer.Transform)),
                    typeof(PropertyGroupDescriptionAttribute)
                );
                LayerPropertyGroups.Add(new LayerPropertyGroupViewModel(ProfileEditorService, layer.General, (PropertyGroupDescriptionAttribute) generalAttribute));
                LayerPropertyGroups.Add(new LayerPropertyGroupViewModel(ProfileEditorService, layer.Transform, (PropertyGroupDescriptionAttribute) transformAttribute));

                if (layer.LayerBrush != null)
                {
                    // Add the rout group of the brush
                    // The root group of the brush has no attribute so let's pull one out of our sleeve
                    var brushDescription = new PropertyGroupDescriptionAttribute
                    {
                        Name = layer.LayerBrush.Descriptor.DisplayName,
                        Description = layer.LayerBrush.Descriptor.Description
                    };
                    LayerPropertyGroups.Add(new LayerPropertyGroupViewModel(ProfileEditorService, layer.LayerBrush.BaseProperties, brushDescription));
                }
            }
            else
                SelectedLayer = null;

            TreeViewModel = new TreeViewModel(this, LayerPropertyGroups);
            TimelineViewModel = new TimelineViewModel(this, LayerPropertyGroups);
        }

        private void SelectedLayerOnLayerBrushUpdated(object sender, EventArgs e)
        {
            // Get rid of the old layer properties group
            if (LayerPropertyGroups.Count == 3)
            {
                LayerPropertyGroups[2].Dispose();
                LayerPropertyGroups.RemoveAt(2);
            }

            if (SelectedLayer.LayerBrush != null)
            {
                // Add the rout group of the brush
                // The root group of the brush has no attribute so let's pull one out of our sleeve
                var brushDescription = new PropertyGroupDescriptionAttribute
                {
                    Name = SelectedLayer.LayerBrush.Descriptor.DisplayName,
                    Description = SelectedLayer.LayerBrush.Descriptor.Description
                };
                LayerPropertyGroups.Add(new LayerPropertyGroupViewModel(ProfileEditorService, SelectedLayer.LayerBrush.BaseProperties, brushDescription));
            }
        }

        #endregion

        #region Controls

        public void PlayFromStart()
        {
            if (!Playing)
                ProfileEditorService.CurrentTime = TimeSpan.Zero;

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

            CoreService.FrameRendering += CoreServiceOnFrameRendering;
            Playing = true;
        }

        public void Pause()
        {
            if (!Playing)
                return;

            CoreService.FrameRendering -= CoreServiceOnFrameRendering;
            Playing = false;
        }


        public void GoToStart()
        {
            ProfileEditorService.CurrentTime = TimeSpan.Zero;
        }

        public void GoToEnd()
        {
            ProfileEditorService.CurrentTime = CalculateEndTime();
        }

        public void GoToPreviousFrame()
        {
            var frameTime = 1000.0 / SettingsService.GetSetting("Core.TargetFrameRate", 25).Value;
            var newTime = Math.Max(0, Math.Round((ProfileEditorService.CurrentTime.TotalMilliseconds - frameTime) / frameTime) * frameTime);
            ProfileEditorService.CurrentTime = TimeSpan.FromMilliseconds(newTime);
        }

        public void GoToNextFrame()
        {
            var frameTime = 1000.0 / SettingsService.GetSetting("Core.TargetFrameRate", 25).Value;
            var newTime = Math.Round((ProfileEditorService.CurrentTime.TotalMilliseconds + frameTime) / frameTime) * frameTime;
            newTime = Math.Min(newTime, CalculateEndTime().TotalMilliseconds);
            ProfileEditorService.CurrentTime = TimeSpan.FromMilliseconds(newTime);
        }

        private TimeSpan CalculateEndTime()
        {
            if (!(ProfileEditorService.SelectedProfileElement is Layer layer))
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
                var newTime = ProfileEditorService.CurrentTime.Add(TimeSpan.FromSeconds(e.DeltaTime));
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

                ProfileEditorService.CurrentTime = newTime;
            });
        }

        #endregion

        #region Caret movement

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
                var newTime = TimeSpan.FromSeconds(x / ProfileEditorService.PixelsPerSecond);

                // Round the time to something that fits the current zoom level
                if (ProfileEditorService.PixelsPerSecond < 200)
                    newTime = TimeSpan.FromMilliseconds(Math.Round(newTime.TotalMilliseconds / 5.0) * 5.0);
                else if (ProfileEditorService.PixelsPerSecond < 500)
                    newTime = TimeSpan.FromMilliseconds(Math.Round(newTime.TotalMilliseconds / 2.0) * 2.0);
                else
                    newTime = TimeSpan.FromMilliseconds(Math.Round(newTime.TotalMilliseconds));

                if (!Keyboard.IsKeyDown(Key.LeftShift) && !Keyboard.IsKeyDown(Key.RightShift))
                {
                    ProfileEditorService.CurrentTime = newTime;
                    return;
                }

                var visibleKeyframes = GetKeyframes(true);

                // Take a tolerance of 5 pixels (half a keyframe width)
                var tolerance = 1000f / ProfileEditorService.PixelsPerSecond * 5;
                var closeKeyframe = visibleKeyframes.FirstOrDefault(k => Math.Abs(k.Position.TotalMilliseconds - newTime.TotalMilliseconds) < tolerance);
                ProfileEditorService.CurrentTime = closeKeyframe?.Position ?? newTime;
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
    }
}
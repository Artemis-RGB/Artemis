using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Artemis.Core.Events;
using Artemis.Core.Models.Profile;
using Artemis.Core.Services;
using Artemis.Core.Services.Interfaces;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.PropertyTree;
using Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.Timeline;
using Artemis.UI.Services.Interfaces;
using Stylet;

namespace Artemis.UI.Screens.Module.ProfileEditor.LayerProperties
{
    public class LayerPropertiesViewModel : ProfileEditorPanelViewModel
    {
        private readonly ICoreService _coreService;
        private readonly ILayerPropertyViewModelFactory _layerPropertyViewModelFactory;
        private readonly IProfileEditorService _profileEditorService;
        private readonly ISettingsService _settingsService;

        public LayerPropertiesViewModel(IProfileEditorService profileEditorService,
            ICoreService coreService,
            ISettingsService settingsService,
            ILayerPropertyViewModelFactory layerPropertyViewModelFactory,
            IPropertyTreeViewModelFactory propertyTreeViewModelFactory,
            IPropertyTimelineViewModelFactory propertyTimelineViewModelFactory)
        {
            _profileEditorService = profileEditorService;
            _coreService = coreService;
            _settingsService = settingsService;
            _layerPropertyViewModelFactory = layerPropertyViewModelFactory;

            PixelsPerSecond = 31;
            PropertyTree = propertyTreeViewModelFactory.Create(this);
            PropertyTimeline = propertyTimelineViewModelFactory.Create(this);

            PopulateProperties();

            _profileEditorService.SelectedProfileElementChanged += (sender, args) => PopulateProperties();
            _profileEditorService.CurrentTimeChanged += ProfileEditorServiceOnCurrentTimeChanged;
        }

        public bool Playing { get; set; }

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

        public PropertyTreeViewModel PropertyTree { get; set; }
        public PropertyTimelineViewModel PropertyTimeline { get; set; }

        private void PopulateProperties()
        {
            if (_profileEditorService.SelectedProfileElement is Layer selectedLayer)
            {
                // Only create VMs for top-level parents, let parents populate their own children recursively
                var propertyViewModels = selectedLayer.Properties
                    .Where(p => p.Children.Any())
                    .Select(p => _layerPropertyViewModelFactory.Create(p, null))
                    .ToList();

                PropertyTree.PopulateProperties(propertyViewModels);
                PropertyTimeline.PopulateProperties(propertyViewModels);
            }
            else
            {
                PropertyTree.ClearProperties();
                PropertyTimeline.ClearProperties();
            }
        }

        private void ProfileEditorServiceOnCurrentTimeChanged(object sender, EventArgs e)
        {
            NotifyOfPropertyChange(() => FormattedCurrentTime);
            NotifyOfPropertyChange(() => TimeCaretPosition);
        }

        protected override void OnDeactivate()
        {
            Pause();
            base.OnDeactivate();
        }

        #region Controls

        public void PlayFromStart()
        {
            if (!IsActive)
                return;
            if (Playing)
                Pause();

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
            // End time is the last keyframe + 10 sec
            var lastKeyFrame = PropertyTimeline.PropertyTrackViewModels.SelectMany(r => r.KeyframeViewModels).OrderByDescending(t => t.Keyframe.Position).FirstOrDefault();
            return lastKeyFrame?.Keyframe.Position.Add(new TimeSpan(0, 0, 0, 10)) ?? TimeSpan.FromSeconds(10);
        }

        private void CoreServiceOnFrameRendering(object sender, FrameRenderingEventArgs e)
        {
            Execute.PostToUIThread(() =>
            {
                var newTime = _profileEditorService.CurrentTime.Add(TimeSpan.FromSeconds(e.DeltaTime));
                if (newTime > CalculateEndTime())
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

                // If shift is held, snap to closest keyframe
                var visibleKeyframes = PropertyTimeline.PropertyTrackViewModels
                    .Where(t => t.LayerPropertyViewModel.Parent != null && t.LayerPropertyViewModel.Parent.IsExpanded)
                    .SelectMany(t => t.KeyframeViewModels);
                // Take a tolerance of 5 pixels (half a keyframe width)
                var tolerance = 1000f / PixelsPerSecond * 5;
                var closeKeyframe = visibleKeyframes.FirstOrDefault(
                    kf => Math.Abs(kf.Keyframe.Position.TotalMilliseconds - newTime.TotalMilliseconds) < tolerance
                );
                _profileEditorService.CurrentTime = closeKeyframe?.Keyframe.Position ?? newTime;
            }
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
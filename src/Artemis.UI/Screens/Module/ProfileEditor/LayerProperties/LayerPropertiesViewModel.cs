using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Artemis.Core.Events;
using Artemis.Core.Models.Profile;
using Artemis.Core.Models.Profile.LayerProperties;
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
        private readonly List<LayerPropertyViewModel> _layerPropertyViewModels;
        private readonly ILayerPropertyVmFactory _layerPropertyVmFactory;
        private readonly IProfileEditorService _profileEditorService;
        private readonly ISettingsService _settingsService;

        public LayerPropertiesViewModel(IProfileEditorService profileEditorService,
            ICoreService coreService,
            ISettingsService settingsService,
            ILayerPropertyVmFactory layerPropertyVmFactory,
            IPropertyTreeVmFactory propertyTreeVmFactory,
            IPropertyTimelineVmFactory propertyTimelineVmFactory)
        {
            _profileEditorService = profileEditorService;
            _coreService = coreService;
            _settingsService = settingsService;
            _layerPropertyVmFactory = layerPropertyVmFactory;
            _layerPropertyViewModels = new List<LayerPropertyViewModel>();

            PixelsPerSecond = 31;
            PropertyTree = propertyTreeVmFactory.Create(this);
            PropertyTimeline = propertyTimelineVmFactory.Create(this);

            PopulateProperties(_profileEditorService.SelectedProfileElement, null);
            _profileEditorService.SelectedProfileElementChanged += (sender, args) => PopulateProperties(args.ProfileElement, args.PreviousProfileElement);
            _profileEditorService.SelectedProfileChanged += (sender, args) => PopulateProperties(_profileEditorService.SelectedProfileElement, null);
            _profileEditorService.CurrentTimeChanged += ProfileEditorServiceOnCurrentTimeChanged;
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

        public PropertyTreeViewModel PropertyTree { get; set; }
        public PropertyTimelineViewModel PropertyTimeline { get; set; }

        protected override void OnDeactivate()
        {
            Pause();
            base.OnDeactivate();
        }

        private void ProfileEditorServiceOnCurrentTimeChanged(object sender, EventArgs e)
        {
            NotifyOfPropertyChange(() => FormattedCurrentTime);
            NotifyOfPropertyChange(() => TimeCaretPosition);
        }

        #region View model managament

        private void PopulateProperties(ProfileElement profileElement, ProfileElement previousProfileElement)
        {
            if (previousProfileElement is Layer previousLayer)
            {
                previousLayer.LayerPropertyRegistered -= LayerOnPropertyRegistered;
                previousLayer.LayerPropertyRemoved -= LayerOnPropertyRemoved;
            }

            if (profileElement is Layer layer)
            {
                // Create VMs for missing properties
                foreach (var baseLayerProperty in layer.Properties)
                {
                    if (_layerPropertyViewModels.All(vm => vm.LayerProperty != baseLayerProperty))
                        CreatePropertyViewModel(baseLayerProperty);
                }

                // Remove VMs for extra properties
                foreach (var layerPropertyViewModel in _layerPropertyViewModels.ToList())
                {
                    if (layer.Properties.All(p => p != layerPropertyViewModel.LayerProperty))
                        RemovePropertyViewModel(layerPropertyViewModel);
                }

                layer.LayerPropertyRegistered += LayerOnPropertyRegistered;
                layer.LayerPropertyRemoved += LayerOnPropertyRemoved;
            }
        }

        private void LayerOnPropertyRegistered(object sender, LayerPropertyEventArgs e)
        {
            Console.WriteLine("LayerOnPropertyRegistered");
            PopulateProperties(e.LayerProperty.Layer, e.LayerProperty.Layer);
        }

        private void LayerOnPropertyRemoved(object sender, LayerPropertyEventArgs e)
        {
            Console.WriteLine("LayerOnPropertyRemoved");
            PopulateProperties(e.LayerProperty.Layer, e.LayerProperty.Layer);
        }

        private LayerPropertyViewModel CreatePropertyViewModel(BaseLayerProperty layerProperty)
        {
            LayerPropertyViewModel parent = null;
            // If the property has a parent, find it's VM
            if (layerProperty.Parent != null)
            {
                parent = _layerPropertyViewModels.FirstOrDefault(vm => vm.LayerProperty == layerProperty.Parent);
                // If no VM is found, create it
                if (parent == null)
                    parent = CreatePropertyViewModel(layerProperty.Parent);
            }

            var createdViewModel = _layerPropertyVmFactory.Create(layerProperty, parent);
            _layerPropertyViewModels.Add(createdViewModel);
            PropertyTree.AddLayerProperty(createdViewModel);
            PropertyTimeline.AddLayerProperty(createdViewModel);

            return createdViewModel;
        }

        private void RemovePropertyViewModel(LayerPropertyViewModel layerPropertyViewModel)
        {
            PropertyTree.RemoveLayerProperty(layerPropertyViewModel);
            PropertyTimeline.RemoveLayerProperty(layerPropertyViewModel);
            _layerPropertyViewModels.Remove(layerPropertyViewModel);
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
            // End time is the last keyframe + 10 sec
            var lastKeyFrame = PropertyTimeline.PropertyTrackViewModels.SelectMany(r => r.KeyframeViewModels).OrderByDescending(t => t.Keyframe.Position).FirstOrDefault();
            return lastKeyFrame?.Keyframe.Position.Add(new TimeSpan(0, 0, 0, 10)) ?? TimeSpan.FromSeconds(10);
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
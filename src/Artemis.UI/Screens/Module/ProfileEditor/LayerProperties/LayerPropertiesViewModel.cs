using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Artemis.Core.Models.Profile;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.PropertyTree;
using Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.Timeline;
using Artemis.UI.Services.Interfaces;

namespace Artemis.UI.Screens.Module.ProfileEditor.LayerProperties
{
    public class LayerPropertiesViewModel : ProfileEditorPanelViewModel
    {
        private readonly ILayerPropertyViewModelFactory _layerPropertyViewModelFactory;
        private readonly IProfileEditorService _profileEditorService;

        public LayerPropertiesViewModel(IProfileEditorService profileEditorService,
            ILayerPropertyViewModelFactory layerPropertyViewModelFactory,
            IPropertyTreeViewModelFactory propertyTreeViewModelFactory,
            IPropertyTimelineViewModelFactory propertyTimelineViewModelFactory)
        {
            _profileEditorService = profileEditorService;
            _layerPropertyViewModelFactory = layerPropertyViewModelFactory;

            PixelsPerSecond = 1;
            PropertyTree = propertyTreeViewModelFactory.Create(this);
            PropertyTimeline = propertyTimelineViewModelFactory.Create(this);

            PopulateProperties();

            _profileEditorService.SelectedProfileElementChanged += (sender, args) => PopulateProperties();
            _profileEditorService.CurrentTimeChanged += ProfileEditorServiceOnCurrentTimeChanged;
        }

        public string FormattedCurrentTime
        {
            get
            {
                if (PixelsPerSecond > 200)
                    return $"{Math.Floor(_profileEditorService.CurrentTime.TotalSeconds):00}.{_profileEditorService.CurrentTime.Milliseconds:000}";
                if (PixelsPerSecond > 60)
                    return $"{Math.Floor(_profileEditorService.CurrentTime.TotalSeconds):00}.{_profileEditorService.CurrentTime.Milliseconds:000}";
                return $"{Math.Floor(_profileEditorService.CurrentTime.TotalMinutes):0}:{_profileEditorService.CurrentTime.Seconds:00}";
            }
        }

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

        #region Caret movement

        private double _caretStartMouseStartOffset;
        private bool _mouseOverCaret;
        private int _pixelsPerSecond;

        public void TimelineMouseDown(object sender, MouseButtonEventArgs e)
        {
            // TODO Preserve mouse offset
            _caretStartMouseStartOffset = e.GetPosition((IInputElement) sender).X - TimeCaretPosition.Left;
        }

        public void CaretMouseEnter(object sender, MouseEventArgs e)
        {
            _mouseOverCaret = true;
        }

        public void CaretMouseLeave(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
                _mouseOverCaret = false;
        }

        public void TimelineMouseMove(object sender, MouseEventArgs e)
        {
            if (_mouseOverCaret && e.LeftButton == MouseButtonState.Pressed)
            {
                // Snap to visible keyframes
                var visibleKeyframes = PropertyTimeline.PropertyTrackViewModels.Where(t => t.LayerPropertyViewModel.Parent != null &&
                                                                                           t.LayerPropertyViewModel.Parent.IsExpanded)
                    .SelectMany(t => t.KeyframeViewModels);

                TimeCaretPosition = new Thickness(Math.Max(0, e.GetPosition((IInputElement) sender).X + _caretStartMouseStartOffset), 0, 0, 0);


                // Take a tolerance of 5 pixels (half a keyframe width)
                var tolerance = 1000f / PixelsPerSecond * 5;
                var closeKeyframe = visibleKeyframes.FirstOrDefault(
                    kf => Math.Abs(kf.Keyframe.Position.TotalMilliseconds - _profileEditorService.CurrentTime.TotalMilliseconds) < tolerance
                );
                if (closeKeyframe != null)
                    _profileEditorService.CurrentTime = closeKeyframe.Keyframe.Position;
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
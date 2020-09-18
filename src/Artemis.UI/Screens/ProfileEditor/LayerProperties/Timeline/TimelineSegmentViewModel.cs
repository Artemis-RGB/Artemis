using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Artemis.Core;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.LayerProperties.Timeline
{
    public class TimelineSegmentViewModel : Screen, IDisposable
    {
        private bool _draggingSegment;
        private bool _showDisableButton;
        private bool _showRepeatButton;
        private bool _showSegmentName;
        private TimeSpan _segmentLength;
        private TimeSpan _segmentStart;
        private TimeSpan _segmentEnd;
        private double _segmentWidth;
        private bool _segmentEnabled;
        private double _segmentStartPosition;

        public TimelineSegmentViewModel(SegmentViewModelType segment, BindableCollection<LayerPropertyGroupViewModel> layerPropertyGroups,
            IProfileEditorService profileEditorService)
        {
            ProfileEditorService = profileEditorService;
            Segment = segment;
            LayerPropertyGroups = layerPropertyGroups;

            if (Segment == SegmentViewModelType.Start)
                ToolTip = "This segment is played when a layer starts displaying because it's conditions are met";
            else if (Segment == SegmentViewModelType.Main)
                ToolTip = "This segment is played while a condition is met, either once or on a repeating loop";
            else if (Segment == SegmentViewModelType.End)
                ToolTip = "This segment is played once a condition is no longer met";
            IsMainSegment = Segment == SegmentViewModelType.Main;

            ProfileEditorService.PixelsPerSecondChanged += ProfileEditorServiceOnPixelsPerSecondChanged;
            ProfileEditorService.ProfileElementSelected += ProfileEditorServiceOnProfileElementSelected;
            if (ProfileEditorService.SelectedProfileElement != null)
                ProfileEditorService.SelectedProfileElement.PropertyChanged += SelectedProfileElementOnPropertyChanged;

            Update();
        }

        public RenderProfileElement SelectedProfileElement => ProfileEditorService.SelectedProfileElement;
        public SegmentViewModelType Segment { get; }
        public BindableCollection<LayerPropertyGroupViewModel> LayerPropertyGroups { get; }
        public IProfileEditorService ProfileEditorService { get; }

        public string ToolTip { get; }
        public bool IsMainSegment { get; }

        public TimeSpan SegmentLength
        {
            get => _segmentLength;
            set => SetAndNotify(ref _segmentLength, value);
        }

        public TimeSpan SegmentStart
        {
            get => _segmentStart;
            set => SetAndNotify(ref _segmentStart, value);
        }

        public TimeSpan SegmentEnd
        {
            get => _segmentEnd;
            set => SetAndNotify(ref _segmentEnd, value);
        }

        public double SegmentWidth
        {
            get => _segmentWidth;
            set => SetAndNotify(ref _segmentWidth, value);
        }

        public double SegmentStartPosition
        {
            get => _segmentStartPosition;
            set => SetAndNotify(ref _segmentStartPosition, value);
        }

        public bool SegmentEnabled
        {
            get => _segmentEnabled;
            set => SetAndNotify(ref _segmentEnabled, value);
        }

        // Only the main segment supports this, for any other segment the getter always returns false and the setter does nothing
        public bool RepeatSegment
        {
            get
            {
                if (Segment != SegmentViewModelType.Main)
                    return false;

                return SelectedProfileElement?.DisplayContinuously ?? false;
            }
            set
            {
                if (Segment != SegmentViewModelType.Main)
                    return;

                SelectedProfileElement.DisplayContinuously = value;
                ProfileEditorService.UpdateSelectedProfileElement();
                NotifyOfPropertyChange(nameof(RepeatSegment));
            }
        }

        public bool ShowSegmentName
        {
            get => _showSegmentName;
            set => SetAndNotify(ref _showSegmentName, value);
        }

        public bool ShowRepeatButton
        {
            get => _showRepeatButton;
            set => SetAndNotify(ref _showRepeatButton, value);
        }

        public bool ShowDisableButton
        {
            get => _showDisableButton;
            set => SetAndNotify(ref _showDisableButton, value);
        }

        #region Updating

        private void UpdateHeader()
        {
            if (!IsMainSegment)
                ShowSegmentName = SegmentWidth > 60;
            else
                ShowSegmentName = SegmentWidth > 80;

            ShowRepeatButton = SegmentWidth > 45 && IsMainSegment;
            ShowDisableButton = SegmentWidth > 25;
        }

        private void Update()
        {
            if (SelectedProfileElement == null)
            {
                SegmentLength = TimeSpan.Zero;
                SegmentStart = TimeSpan.Zero;
                SegmentEnd = TimeSpan.Zero;
                SegmentStartPosition = 0;
                SegmentWidth = 0;
                SegmentEnabled = false;
                return;
            }

            if (Segment == SegmentViewModelType.Start)
            {
                SegmentLength = SelectedProfileElement.StartSegmentLength;
                SegmentStart = TimeSpan.Zero;
            }
            else if (Segment == SegmentViewModelType.Main)
            {
                SegmentLength = SelectedProfileElement.MainSegmentLength;
                SegmentStart = SelectedProfileElement.StartSegmentLength;
            }
            else if (Segment == SegmentViewModelType.End)
            {
                SegmentLength = SelectedProfileElement.EndSegmentLength;
                SegmentStart = SelectedProfileElement.StartSegmentLength + SelectedProfileElement.MainSegmentLength;
            }

            SegmentEnd = SegmentStart + SegmentLength;
            SegmentStartPosition = SegmentStart.TotalSeconds * ProfileEditorService.PixelsPerSecond;
            SegmentWidth = SegmentLength.TotalSeconds * ProfileEditorService.PixelsPerSecond;
            SegmentEnabled = SegmentLength != TimeSpan.Zero;

            UpdateHeader();
        }

        #endregion

        #region Controls

        public void DisableSegment()
        {
            var startSegmentEnd = SelectedProfileElement.StartSegmentLength;
            var mainSegmentEnd = SelectedProfileElement.StartSegmentLength + SelectedProfileElement.MainSegmentLength;

            var oldSegmentLength = SegmentLength;

            if (Segment == SegmentViewModelType.Start)
            {
                // Remove keyframes that fall in this segment
                WipeKeyframes(null, startSegmentEnd);
                SelectedProfileElement.StartSegmentLength = TimeSpan.Zero;
            }
            else if (Segment == SegmentViewModelType.Main)
            {
                // Remove keyframes that fall in this segment
                WipeKeyframes(startSegmentEnd, startSegmentEnd);
                SelectedProfileElement.MainSegmentLength = TimeSpan.Zero;
            }
            else if (Segment == SegmentViewModelType.End)
            {
                // Remove keyframes that fall in this segment
                WipeKeyframes(mainSegmentEnd, null);
                SelectedProfileElement.EndSegmentLength = TimeSpan.Zero;
            }

            ShiftNextSegment(SegmentLength - oldSegmentLength);
            ProfileEditorService.UpdateSelectedProfileElement();
            Update();
        }

        public void EnableSegment()
        {
            ShiftNextSegment(TimeSpan.FromSeconds(1));
            if (Segment == SegmentViewModelType.Start)
                SelectedProfileElement.StartSegmentLength = TimeSpan.FromSeconds(1);
            else if (Segment == SegmentViewModelType.Main)
                SelectedProfileElement.MainSegmentLength = TimeSpan.FromSeconds(1);
            else if (Segment == SegmentViewModelType.End)
                SelectedProfileElement.EndSegmentLength = TimeSpan.FromSeconds(1);

            ProfileEditorService.UpdateSelectedProfileElement();
            Update();
        }

        #endregion

        #region Mouse events

        public void SegmentMouseDown(object sender, MouseButtonEventArgs e)
        {
            ((IInputElement) sender).CaptureMouse();
            _draggingSegment = true;
        }

        public void SegmentMouseUp(object sender, MouseButtonEventArgs e)
        {
            ((IInputElement) sender).ReleaseMouseCapture();
            _draggingSegment = false;

            ProfileEditorService.UpdateSelectedProfileElement();
        }

        public void SegmentMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed || !_draggingSegment)
                return;

            // Get the parent scroll viewer, need that for our position
            var parent = VisualTreeUtilities.FindParent<ScrollViewer>((DependencyObject) sender, "TimelineHeaderScrollViewer");

            var x = Math.Max(0, e.GetPosition(parent).X);
            var newTime = TimeSpan.FromSeconds(x / ProfileEditorService.PixelsPerSecond);

            // Round the time to something that fits the current zoom level
            if (ProfileEditorService.PixelsPerSecond < 200)
                newTime = TimeSpan.FromMilliseconds(Math.Round(newTime.TotalMilliseconds / 5.0) * 5.0);
            else if (ProfileEditorService.PixelsPerSecond < 500)
                newTime = TimeSpan.FromMilliseconds(Math.Round(newTime.TotalMilliseconds / 2.0) * 2.0);
            else
                newTime = TimeSpan.FromMilliseconds(Math.Round(newTime.TotalMilliseconds));

            // If holding down shift, snap to the closest element on the timeline
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                var keyframeTimes = LayerPropertyGroups.SelectMany(g => g.GetAllKeyframeViewModels(true)).Select(k => k.Position).ToList();
                newTime = ProfileEditorService.SnapToTimeline(newTime, TimeSpan.FromMilliseconds(1000f / ProfileEditorService.PixelsPerSecond * 5), false, true, keyframeTimes);
            }
            // If holding down control, round to the closest 50ms
            else if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                newTime = TimeSpan.FromMilliseconds(Math.Round(newTime.TotalMilliseconds / 50.0) * 50.0);

            var oldSegmentLength = SegmentLength;
            if (Segment == SegmentViewModelType.Start)
            {
                if (newTime < TimeSpan.FromMilliseconds(100))
                    newTime = TimeSpan.FromMilliseconds(100);
                SelectedProfileElement.StartSegmentLength = newTime;
            }
            else if (Segment == SegmentViewModelType.Main)
            {
                if (newTime < SelectedProfileElement.StartSegmentLength + TimeSpan.FromMilliseconds(100))
                    newTime = SelectedProfileElement.StartSegmentLength + TimeSpan.FromMilliseconds(100);
                SelectedProfileElement.MainSegmentLength = newTime - SelectedProfileElement.StartSegmentLength;
            }
            else if (Segment == SegmentViewModelType.End)
            {
                if (newTime < SelectedProfileElement.StartSegmentLength + SelectedProfileElement.MainSegmentLength + TimeSpan.FromMilliseconds(100))
                    newTime = SelectedProfileElement.StartSegmentLength + SelectedProfileElement.MainSegmentLength + TimeSpan.FromMilliseconds(100);
                SelectedProfileElement.EndSegmentLength = newTime - SelectedProfileElement.StartSegmentLength - SelectedProfileElement.MainSegmentLength;
            }

            ShiftNextSegment(SegmentLength - oldSegmentLength);
            Update();
        }

        #endregion

        #region IDIsposable

        public void Dispose()
        {
            ProfileEditorService.PixelsPerSecondChanged -= ProfileEditorServiceOnPixelsPerSecondChanged;
            ProfileEditorService.ProfileElementSelected -= ProfileEditorServiceOnProfileElementSelected;
            if (SelectedProfileElement != null)
                SelectedProfileElement.PropertyChanged -= SelectedProfileElementOnPropertyChanged;
        }

        #endregion

        #region Event handlers

        private void ProfileEditorServiceOnProfileElementSelected(object? sender, RenderProfileElementEventArgs e)
        {
            if (e.PreviousRenderProfileElement != null)
                e.PreviousRenderProfileElement.PropertyChanged -= SelectedProfileElementOnPropertyChanged;
            if (e.RenderProfileElement != null)
                e.RenderProfileElement.PropertyChanged += SelectedProfileElementOnPropertyChanged;

            Update();
        }

        private void SelectedProfileElementOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(RenderProfileElement.StartSegmentLength) ||
                e.PropertyName == nameof(RenderProfileElement.MainSegmentLength) ||
                e.PropertyName == nameof(RenderProfileElement.EndSegmentLength))
                Update();
            else if (e.PropertyName == nameof(RenderProfileElement.DisplayContinuously))
                NotifyOfPropertyChange(nameof(RepeatSegment));
        }

        private void ProfileEditorServiceOnPixelsPerSecondChanged(object? sender, EventArgs e)
        {
            Update();
        }

        #endregion

        public void ShiftNextSegment(TimeSpan amount)
        {
            var segmentEnd = TimeSpan.Zero;
            if (Segment == SegmentViewModelType.Start)
                segmentEnd = SelectedProfileElement.StartSegmentLength;
            else if (Segment == SegmentViewModelType.Main)
                segmentEnd = SelectedProfileElement.StartSegmentLength + SelectedProfileElement.MainSegmentLength;
            else if (Segment == SegmentViewModelType.End)
                segmentEnd = SelectedProfileElement.TimelineLength;

            ShiftKeyframes(segmentEnd, null, amount);
        }

        private void WipeKeyframes(TimeSpan? start, TimeSpan? end)
        {
            foreach (var layerPropertyGroupViewModel in LayerPropertyGroups)
                layerPropertyGroupViewModel.WipeKeyframes(start, end);
        }

        private void ShiftKeyframes(TimeSpan? start, TimeSpan? end, TimeSpan amount)
        {
            foreach (var layerPropertyGroupViewModel in LayerPropertyGroups)
                layerPropertyGroupViewModel.ShiftKeyframes(start, end, amount);
        }
    }

    public enum SegmentViewModelType
    {
        Start,
        Main,
        End
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Artemis.Core;
using Artemis.UI.Screens.ProfileEditor.LayerProperties.Timeline.Dialogs;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.LayerProperties.Timeline
{
    public sealed class TimelineSegmentViewModel : Screen
    {
        private readonly IDialogService _dialogService;
        private bool _draggingSegment;
        private bool _segmentEnabled;
        private TimeSpan _segmentEnd;
        private TimeSpan _segmentLength;
        private TimeSpan _segmentStart;
        private double _segmentStartPosition;
        private double _segmentWidth;
        private bool _showDisableButton;
        private bool _showRepeatButton;
        private bool _showSegmentName;

        public TimelineSegmentViewModel(SegmentViewModelType segment, IObservableCollection<LayerPropertyGroupViewModel> layerPropertyGroups,
            IProfileEditorService profileEditorService, IDialogService dialogService)
        {
            _dialogService = dialogService;
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
        }

        public RenderProfileElement SelectedProfileElement => ProfileEditorService.SelectedProfileElement;
        public SegmentViewModelType Segment { get; }
        public IObservableCollection<LayerPropertyGroupViewModel> LayerPropertyGroups { get; }
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
                return SelectedProfileElement?.Timeline.PlayMode == TimelinePlayMode.Repeat;
            }
            set
            {
                if (Segment != SegmentViewModelType.Main || SelectedProfileElement == null)
                    return;
                SelectedProfileElement.Timeline.PlayMode = value ? TimelinePlayMode.Repeat : TimelinePlayMode.Once;
                ProfileEditorService.SaveSelectedProfileElement();
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

        public async Task OpenSettingsDialog()
        {
            await _dialogService.ShowDialog<TimelineSegmentDialogViewModel>(new Dictionary<string, object> {{"segment", this}});
        }

        public void ShiftNextSegment(TimeSpan amount)
        {
            if (Segment == SegmentViewModelType.Start)
                ShiftKeyframes(SelectedProfileElement.Timeline.StartSegmentEndPosition, null, amount);
            else if (Segment == SegmentViewModelType.Main)
                ShiftKeyframes(SelectedProfileElement.Timeline.MainSegmentEndPosition, null, amount);
            else if (Segment == SegmentViewModelType.End)
                ShiftKeyframes(SelectedProfileElement.Timeline.EndSegmentEndPosition, null, amount);
        }

        private void WipeKeyframes(TimeSpan? start, TimeSpan? end)
        {
            foreach (LayerPropertyGroupViewModel layerPropertyGroupViewModel in LayerPropertyGroups)
                layerPropertyGroupViewModel.WipeKeyframes(start, end);
        }

        private void ShiftKeyframes(TimeSpan? start, TimeSpan? end, TimeSpan amount)
        {
            foreach (LayerPropertyGroupViewModel layerPropertyGroupViewModel in LayerPropertyGroups)
                layerPropertyGroupViewModel.ShiftKeyframes(start, end, amount);
        }

        #region Overrides of Screen

        /// <inheritdoc />
        protected override void OnInitialActivate()
        {
            ProfileEditorService.PixelsPerSecondChanged += ProfileEditorServiceOnPixelsPerSecondChanged;
            ProfileEditorService.SelectedProfileElementChanged += SelectedProfileEditorServiceOnSelectedProfileElementChanged;
            if (ProfileEditorService.SelectedProfileElement != null)
                ProfileEditorService.SelectedProfileElement.Timeline.PropertyChanged += TimelineOnPropertyChanged;

            Update();
            base.OnInitialActivate();
        }

        /// <inheritdoc />
        protected override void OnClose()
        {
            ProfileEditorService.PixelsPerSecondChanged -= ProfileEditorServiceOnPixelsPerSecondChanged;
            ProfileEditorService.SelectedProfileElementChanged -= SelectedProfileEditorServiceOnSelectedProfileElementChanged;
            if (SelectedProfileElement != null)
                SelectedProfileElement.Timeline.PropertyChanged -= TimelineOnPropertyChanged;
            base.OnClose();
        }

        #endregion

        #region Updating

        private void UpdateHeader()
        {
            if (!IsMainSegment)
                ShowSegmentName = SegmentWidth > 60;
            else
                ShowSegmentName = SegmentWidth > 80;

            ShowRepeatButton = SegmentWidth > 45 && IsMainSegment;
            ShowDisableButton = SegmentWidth > 25;

            if (Segment == SegmentViewModelType.Main)
                NotifyOfPropertyChange(nameof(RepeatSegment));
        }

        private void Update()
        {
            if (SelectedProfileElement == null)
            {
                SegmentStart = TimeSpan.Zero;
                SegmentEnd = TimeSpan.Zero;
                SegmentLength = TimeSpan.Zero;
                SegmentStartPosition = 0;
                SegmentWidth = 0;
                SegmentEnabled = false;
                return;
            }

            // It would be nice to do this differently if this patterns end up appearing in more places
            if (Segment == SegmentViewModelType.Start)
            {
                SegmentStart = TimeSpan.Zero;
                SegmentEnd = SelectedProfileElement.Timeline.StartSegmentEndPosition;
                SegmentLength = SelectedProfileElement.Timeline.StartSegmentLength;
            }
            else if (Segment == SegmentViewModelType.Main)
            {
                SegmentStart = SelectedProfileElement.Timeline.MainSegmentStartPosition;
                SegmentEnd = SelectedProfileElement.Timeline.MainSegmentEndPosition;
                SegmentLength = SelectedProfileElement.Timeline.MainSegmentLength;
            }
            else if (Segment == SegmentViewModelType.End)
            {
                SegmentStart = SelectedProfileElement.Timeline.EndSegmentStartPosition;
                SegmentEnd = SelectedProfileElement.Timeline.EndSegmentEndPosition;
                SegmentLength = SelectedProfileElement.Timeline.EndSegmentLength;
            }

            SegmentStartPosition = SegmentStart.TotalSeconds * ProfileEditorService.PixelsPerSecond;
            SegmentWidth = SegmentLength.TotalSeconds * ProfileEditorService.PixelsPerSecond;
            SegmentEnabled = SegmentLength != TimeSpan.Zero;

            UpdateHeader();
        }

        #endregion

        #region Controls

        public void DisableSegment()
        {
            TimeSpan oldSegmentLength = SegmentLength;

            if (Segment == SegmentViewModelType.Start)
            {
                // Remove keyframes that fall in this segment
                WipeKeyframes(null, SelectedProfileElement.Timeline.StartSegmentEndPosition);
                SelectedProfileElement.Timeline.StartSegmentLength = TimeSpan.Zero;
            }
            else if (Segment == SegmentViewModelType.Main)
            {
                // Remove keyframes that fall in this segment
                WipeKeyframes(SelectedProfileElement.Timeline.MainSegmentStartPosition, SelectedProfileElement.Timeline.MainSegmentEndPosition);
                SelectedProfileElement.Timeline.MainSegmentLength = TimeSpan.Zero;
            }
            else if (Segment == SegmentViewModelType.End)
            {
                // Remove keyframes that fall in this segment
                WipeKeyframes(SelectedProfileElement.Timeline.EndSegmentStartPosition, SelectedProfileElement.Timeline.EndSegmentEndPosition);
                SelectedProfileElement.Timeline.EndSegmentLength = TimeSpan.Zero;
            }

            ShiftNextSegment(SegmentLength - oldSegmentLength);
            ProfileEditorService.SaveSelectedProfileElement();
            Update();
        }

        public void EnableSegment()
        {
            ShiftNextSegment(TimeSpan.FromSeconds(1));
            if (Segment == SegmentViewModelType.Start)
                SelectedProfileElement.Timeline.StartSegmentLength = TimeSpan.FromSeconds(1);
            else if (Segment == SegmentViewModelType.Main)
                SelectedProfileElement.Timeline.MainSegmentLength = TimeSpan.FromSeconds(1);
            else if (Segment == SegmentViewModelType.End)
                SelectedProfileElement.Timeline.EndSegmentLength = TimeSpan.FromSeconds(1);

            ProfileEditorService.SaveSelectedProfileElement();
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

            ProfileEditorService.SaveSelectedProfileElement();
        }

        public void SegmentMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed || !_draggingSegment)
                return;

            // Get the parent scroll viewer, need that for our position
            ScrollViewer parent = VisualTreeUtilities.FindParent<ScrollViewer>((DependencyObject) sender, "TimelineHeaderScrollViewer");

            double x = Math.Max(0, e.GetPosition(parent).X);
            TimeSpan newTime = TimeSpan.FromSeconds(x / ProfileEditorService.PixelsPerSecond);

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
                List<TimeSpan> keyframeTimes = LayerPropertyGroups.SelectMany(g => g.GetAllKeyframeViewModels(true)).Select(k => k.Position).ToList();
                newTime = ProfileEditorService.SnapToTimeline(newTime, TimeSpan.FromMilliseconds(1000f / ProfileEditorService.PixelsPerSecond * 5), false, true, keyframeTimes);
            }
            // If holding down control, round to the closest 50ms
            else if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                newTime = TimeSpan.FromMilliseconds(Math.Round(newTime.TotalMilliseconds / 50.0) * 50.0);
            }

            UpdateEndPosition(newTime);
        }

        public void UpdateEndPosition(TimeSpan end)
        {
            if (end < TimeSpan.FromMilliseconds(100))
                end = TimeSpan.FromMilliseconds(100);

            if (Segment == SegmentViewModelType.Start)
                SelectedProfileElement.Timeline.StartSegmentEndPosition = end;
            else if (Segment == SegmentViewModelType.Main)
                SelectedProfileElement.Timeline.MainSegmentEndPosition = end;
            else if (Segment == SegmentViewModelType.End)
                SelectedProfileElement.Timeline.EndSegmentEndPosition = end;

            Update();
        }

        public void UpdateLength(TimeSpan length)
        {
            if (length < TimeSpan.FromMilliseconds(100))
                length = TimeSpan.FromMilliseconds(100);

            if (Segment == SegmentViewModelType.Start)
                SelectedProfileElement.Timeline.StartSegmentLength = length;
            else if (Segment == SegmentViewModelType.Main)
                SelectedProfileElement.Timeline.MainSegmentLength = length;
            else if (Segment == SegmentViewModelType.End)
                SelectedProfileElement.Timeline.EndSegmentLength = length;

            Update();
        }

        #endregion

        #region Event handlers

        private void SelectedProfileEditorServiceOnSelectedProfileElementChanged(object sender, RenderProfileElementEventArgs e)
        {
            if (e.PreviousRenderProfileElement != null)
                e.PreviousRenderProfileElement.Timeline.PropertyChanged -= TimelineOnPropertyChanged;
            if (e.RenderProfileElement != null)
                e.RenderProfileElement.Timeline.PropertyChanged += TimelineOnPropertyChanged;

            Update();
        }

        private void TimelineOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Core.Timeline.StartSegmentLength) ||
                e.PropertyName == nameof(Core.Timeline.MainSegmentLength) ||
                e.PropertyName == nameof(Core.Timeline.EndSegmentLength))
                Update();
            else if (e.PropertyName == nameof(Core.Timeline.PlayMode))
                NotifyOfPropertyChange(nameof(RepeatSegment));
        }

        private void ProfileEditorServiceOnPixelsPerSecondChanged(object sender, EventArgs e)
        {
            Update();
        }

        #endregion
    }

    public enum SegmentViewModelType
    {
        Start,
        Main,
        End
    }
}
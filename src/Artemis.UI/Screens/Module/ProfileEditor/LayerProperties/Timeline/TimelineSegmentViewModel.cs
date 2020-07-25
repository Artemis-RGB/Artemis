using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Artemis.Core.Models.Profile;
using Artemis.UI.Shared.Services.Interfaces;
using Artemis.UI.Shared.Utilities;
using Stylet;

namespace Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.Timeline
{
    public class TimelineSegmentViewModel : PropertyChangedBase, IDisposable
    {
        private bool _draggingSegment;
        private bool _showSegmentName;
        private bool _showRepeatButton;
        private bool _showDisableButton;

        public TimelineSegmentViewModel(IProfileEditorService profileEditorService, SegmentViewModelType segment)
        {
            ProfileEditorService = profileEditorService;
            Segment = segment;
            SelectedProfileElement = ProfileEditorService.SelectedProfileElement;

            switch (Segment)
            {
                case SegmentViewModelType.Start:
                    ToolTip = "This segment is played when a layer starts displaying because it's conditions are met";
                    break;
                case SegmentViewModelType.Main:
                    ToolTip = "This segment is played while a condition is met, either once or on a repeating loop";
                    break;
                case SegmentViewModelType.End:
                    ToolTip = "This segment is played once a condition is no longer met";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(segment));
            }

            UpdateDisplay();
            ProfileEditorService.PixelsPerSecondChanged += ProfileEditorServiceOnPixelsPerSecondChanged;
            SelectedProfileElement.PropertyChanged += SelectedProfileElementOnPropertyChanged;
        }

        public RenderProfileElement SelectedProfileElement { get; }

        public SegmentViewModelType Segment { get; }
        public IProfileEditorService ProfileEditorService { get; }
        public string ToolTip { get; }

        public TimeSpan SegmentLength
        {
            get
            {
                return Segment switch
                {
                    SegmentViewModelType.Start => SelectedProfileElement?.StartSegmentLength ?? TimeSpan.Zero,
                    SegmentViewModelType.Main => SelectedProfileElement?.MainSegmentLength ?? TimeSpan.Zero,
                    SegmentViewModelType.End => SelectedProfileElement?.EndSegmentLength ?? TimeSpan.Zero,
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
        }

        public double SegmentWidth => ProfileEditorService.PixelsPerSecond * SegmentLength.TotalSeconds;

        public bool SegmentEnabled => SegmentLength != TimeSpan.Zero;
        public bool IsMainSegment => Segment == SegmentViewModelType.Main;

        // Only the main segment supports this, for any other segment the getter always returns false and the setter does nothing
        public bool RepeatSegment
        {
            get
            {
                if (Segment != SegmentViewModelType.Main)
                    return false;

                return SelectedProfileElement?.RepeatMainSegment ?? false;
            }
            set
            {
                if (Segment != SegmentViewModelType.Main)
                    return;

                SelectedProfileElement.RepeatMainSegment = value;
                ProfileEditorService.UpdateSelectedProfileElement();
                NotifyOfPropertyChange(nameof(RepeatSegment));
            }
        }

        public double SegmentStartPosition
        {
            get
            {
                return Segment switch
                {
                    SegmentViewModelType.Start => 0,
                    SegmentViewModelType.Main => ProfileEditorService.PixelsPerSecond * SelectedProfileElement.StartSegmentLength.TotalSeconds,
                    SegmentViewModelType.End => ProfileEditorService.PixelsPerSecond * (SelectedProfileElement.StartSegmentLength.TotalSeconds + SelectedProfileElement.MainSegmentLength.TotalSeconds),
                    _ => throw new ArgumentOutOfRangeException()
                };
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

        public void Dispose()
        {
            ProfileEditorService.PixelsPerSecondChanged -= ProfileEditorServiceOnPixelsPerSecondChanged;
            SelectedProfileElement.PropertyChanged -= SelectedProfileElementOnPropertyChanged;
        }

        public void DisableSegment()
        {
            switch (Segment)
            {
                case SegmentViewModelType.Start:
                    SelectedProfileElement.StartSegmentLength = TimeSpan.Zero;
                    break;
                case SegmentViewModelType.Main:
                    SelectedProfileElement.MainSegmentLength = TimeSpan.Zero;
                    break;
                case SegmentViewModelType.End:
                    SelectedProfileElement.EndSegmentLength = TimeSpan.Zero;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            NotifyOfPropertyChange(nameof(SegmentEnabled));
            ProfileEditorService.UpdateSelectedProfileElement();
        }

        public void EnableSegment()
        {
            switch (Segment)
            {
                case SegmentViewModelType.Start:
                    SelectedProfileElement.StartSegmentLength = TimeSpan.FromSeconds(1);
                    break;
                case SegmentViewModelType.Main:
                    SelectedProfileElement.MainSegmentLength = TimeSpan.FromSeconds(1);
                    break;
                case SegmentViewModelType.End:
                    SelectedProfileElement.EndSegmentLength = TimeSpan.FromSeconds(1);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            NotifyOfPropertyChange(nameof(SegmentEnabled));
            ProfileEditorService.UpdateSelectedProfileElement();
        }

        public void SegmentMouseDown(object sender, MouseButtonEventArgs e)
        {
            ((IInputElement) sender).CaptureMouse();
            _draggingSegment = true;
        }

        public void SegmentMouseUp(object sender, MouseButtonEventArgs e)
        {
            ((IInputElement) sender).ReleaseMouseCapture();
            _draggingSegment = false;
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
                newTime = ProfileEditorService.SnapToTimeline(newTime, TimeSpan.FromMilliseconds(1000f / ProfileEditorService.PixelsPerSecond * 5), false, true, true);
            // If holding down control, round to the closest 50ms
            else if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                newTime = TimeSpan.FromMilliseconds(Math.Round(newTime.TotalMilliseconds / 50.0) * 50.0);


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

            NotifyOfPropertyChange(nameof(SegmentLength));
            NotifyOfPropertyChange(nameof(SegmentWidth));

            UpdateDisplay();
        }

        private void SelectedProfileElementOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(RenderProfileElement.StartSegmentLength) || 
                e.PropertyName == nameof(RenderProfileElement.MainSegmentLength) ||
                e.PropertyName == nameof(RenderProfileElement.EndSegmentLength))
            {
                NotifyOfPropertyChange(nameof(SegmentStartPosition));
                NotifyOfPropertyChange(nameof(SegmentWidth));
            }
        }


        private void ProfileEditorServiceOnPixelsPerSecondChanged(object? sender, EventArgs e)
        {
            NotifyOfPropertyChange(nameof(SegmentWidth));
            NotifyOfPropertyChange(nameof(SegmentStartPosition));

            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            if (!IsMainSegment)
                ShowSegmentName = SegmentWidth > 60;
            else
                ShowSegmentName = SegmentWidth > 80;

            ShowRepeatButton = SegmentWidth > 45 && IsMainSegment;
            ShowDisableButton = SegmentWidth > 25;
        }
    }

    public enum SegmentViewModelType
    {
        Start,
        Main,
        End
    }
}
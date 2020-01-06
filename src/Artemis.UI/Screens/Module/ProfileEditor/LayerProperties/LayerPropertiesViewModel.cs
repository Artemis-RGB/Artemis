using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Artemis.Core.Models.Profile;
using Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.PropertyTree;
using Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.Timeline;
using Artemis.UI.Services.Interfaces;

namespace Artemis.UI.Screens.Module.ProfileEditor.LayerProperties
{
    public class LayerPropertiesViewModel : ProfileEditorPanelViewModel
    {
        private readonly IProfileEditorService _profileEditorService;

        public LayerPropertiesViewModel(IProfileEditorService profileEditorService)
        {
            _profileEditorService = profileEditorService;

            CurrentTime = TimeSpan.Zero;
            PixelsPerSecond = 1;
            PropertyTree = new PropertyTreeViewModel(this);
            PropertyTimeline = new PropertyTimelineViewModel(this);

            PopulateProperties();

            _profileEditorService.SelectedProfileElementChanged += (sender, args) => PopulateProperties();
        }

        public TimeSpan CurrentTime
        {
            get => _currentTime;
            set
            {
                _currentTime = value;
                OnCurrentTimeChanged();
            }
        }

        public string FormattedCurrentTime
        {
            get
            {
                if (PixelsPerSecond > 200)
                    return $"{Math.Floor(CurrentTime.TotalSeconds):00}.{CurrentTime.Milliseconds:000}";
                if (PixelsPerSecond > 60)
                    return $"{Math.Floor(CurrentTime.TotalSeconds):00}.{CurrentTime.Milliseconds:000}";
                return $"{Math.Floor(CurrentTime.TotalMinutes):0}:{CurrentTime.Seconds:00}";
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
            get => new Thickness(CurrentTime.TotalSeconds * PixelsPerSecond, 0, 0, 0);
            set => CurrentTime = TimeSpan.FromSeconds(value.Left / PixelsPerSecond);
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
                    .Select(p => new LayerPropertyViewModel(p, null))
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

        #region Caret movement

        private double _caretStartMouseStartOffset;
        private bool _mouseOverCaret;
        private int _pixelsPerSecond;
        private TimeSpan _currentTime;

        public void RightGridMouseDown(object sender, MouseButtonEventArgs e)
        {
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

        public void RightGridMouseMove(object sender, MouseEventArgs e)
        {
            if (_mouseOverCaret && e.LeftButton == MouseButtonState.Pressed)
                TimeCaretPosition = new Thickness(Math.Max(0, e.GetPosition((IInputElement) sender).X), 0, 0, 0);
        }

        #endregion

        #region Events

        public event EventHandler CurrentTimeChanged;
        public event EventHandler PixelsPerSecondChanged;

        protected virtual void OnCurrentTimeChanged()
        {
            CurrentTimeChanged?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnPixelsPerSecondChanged()
        {
            PixelsPerSecondChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}
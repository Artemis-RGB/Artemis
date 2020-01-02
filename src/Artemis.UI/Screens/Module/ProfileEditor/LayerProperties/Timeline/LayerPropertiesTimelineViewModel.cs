using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Markup;
using Stylet;

namespace Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.Timeline
{
    public class LayerPropertiesTimelineViewModel : PropertyChangedBase
    {
        private int _pixelsPerSecond = 1;

        public LayerPropertiesTimelineViewModel()
        {
            TimelinePropertyRailViewModels = new BindableCollection<TimelinePropertyRailViewModel>();

            TimeCaretTime = TimeSpan.FromSeconds(3);
            CreateTestValues();
            Update();
        }

        public int PixelsPerSecond
        {
            get => _pixelsPerSecond;
            set
            {
                _pixelsPerSecond = value;
                Update();
            }
        }

        public TimeSpan TimeCaretTime { get; set; }
        public Thickness TimeCaretPosition
        {
            get => new Thickness(TimeCaretTime.TotalSeconds * PixelsPerSecond, 0, 0, 0);
            set => TimeCaretTime = TimeSpan.FromSeconds(value.Left / PixelsPerSecond);
        }

        public BindableCollection<TimelinePropertyRailViewModel> TimelinePropertyRailViewModels { get; set; }

        private void CreateTestValues()
        {
            var propertyRailViewModels = new List<TimelinePropertyRailViewModel>();
            for (var i = 0; i < 20; i++)
                propertyRailViewModels.Add(new TimelinePropertyRailViewModel());

            TimelinePropertyRailViewModels.AddRange(propertyRailViewModels);
        }

        public void Update()
        {
            foreach (var timelinePropertyRailViewModel in TimelinePropertyRailViewModels)
                timelinePropertyRailViewModel.Update(PixelsPerSecond);
        }
    }
}
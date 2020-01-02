using System;
using System.Collections.Generic;
using System.Linq;
using Stylet;

namespace Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.Timeline
{
    public class TimelinePropertyRailViewModel : PropertyChangedBase
    {
        public TimelinePropertyRailViewModel()
        {
            TimelineKeyframeViewModels = new BindableCollection<TimelineKeyframeViewModel>();
            CreateTestValues();
        }

        public BindableCollection<TimelineKeyframeViewModel> TimelineKeyframeViewModels { get; set; }
        public double Width { get; set; }

        public void CreateTestValues()
        {
            var keyframeViewModels = new List<TimelineKeyframeViewModel>();
            for (var i = 0; i < 20; i++)
            {
                keyframeViewModels.Add(new TimelineKeyframeViewModel());
                keyframeViewModels[i].Position = TimeSpan.FromSeconds(i);
            }

            TimelineKeyframeViewModels.AddRange(keyframeViewModels);
        }

        public void Update(int pixelsPerSecond)
        {
            foreach (var timelineKeyframeViewModel in TimelineKeyframeViewModels)
                timelineKeyframeViewModel.Update(pixelsPerSecond);

            // End time is the last keyframe + 10 sec
            var lastKeyFrame = TimelineKeyframeViewModels.OrderByDescending(t => t.Position).FirstOrDefault();
            var endTime = lastKeyFrame?.Position.Add(new TimeSpan(0, 0, 0, 10)) ?? TimeSpan.FromSeconds(10);

            Width = endTime.TotalSeconds * pixelsPerSecond;
        }
    }
}
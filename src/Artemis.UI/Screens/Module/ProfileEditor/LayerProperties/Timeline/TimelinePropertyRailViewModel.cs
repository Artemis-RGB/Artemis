using System;
using System.Collections.Generic;
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

            if (pixelsPerSecond == 10)
                Console.WriteLine();
        }
    }
}
using System;
using System.Windows;
using Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.PropertyTree;
using Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.Timeline;

namespace Artemis.UI.Screens.Module.ProfileEditor.LayerProperties
{
    public class LayerPropertiesViewModel : ProfileEditorPanelViewModel
    {
        public LayerPropertiesViewModel()
        {
            PropertyTree = new PropertyTreeViewModel(this);
            PropertyTimeline = new PropertyTimelineViewModel(this);
        }

        public TimeSpan CurrentTime { get; set; }
        public int PixelsPerSecond { get; set; }
        public Thickness TimeCaretPosition
        {
            get => new Thickness(CurrentTime.TotalSeconds * PixelsPerSecond, 0, 0, 0);
            set => CurrentTime = TimeSpan.FromSeconds(value.Left / PixelsPerSecond);
        }

        public PropertyTreeViewModel PropertyTree { get; set; }
        public PropertyTimelineViewModel PropertyTimeline { get; set; }

        private void CreateTestValues()
        {
//            var propertyRailViewModels = new List<TimelinePropertyRailViewModel>();
//            for (var i = 0; i < 20; i++)
//                propertyRailViewModels.Add(new TimelinePropertyRailViewModel());
//
//            TimelinePropertyRailViewModels.AddRange(propertyRailViewModels);
//
//            // Transform will be the only built-in properties so test with that
//            var testVm = new TimelinePropertiesViewModel { PropertiesName = "Transform" };
//            testVm.Properties.Add(new TimelinePropertyViewModel { PropertyName = "Anchor Point" });
//            testVm.Properties.Add(new TimelinePropertyViewModel { PropertyName = "Position" });
//            testVm.Properties.Add(new TimelinePropertyViewModel { PropertyName = "Scale" });
//            testVm.Properties.Add(new TimelinePropertyViewModel { PropertyName = "Rotation" });
//            testVm.Properties.Add(new TimelinePropertyViewModel { PropertyName = "Opacity" });
//            // Purely for testing, add a nested property 
//            var subTestVm = new TimelinePropertiesViewModel() { PropertiesName = "Sub Properties" };
//            subTestVm.Properties.Add(new TimelinePropertyViewModel { PropertyName = "Sub Property 1" });
//            subTestVm.Properties.Add(new TimelinePropertyViewModel { PropertyName = "Sub Property 2" });
//            subTestVm.Properties.Add(new TimelinePropertyViewModel { PropertyName = "Sub Property 3" });
//            testVm.Properties.Add(subTestVm);
//
//            TimelinePropertiesViewModels.Add(testVm);
        }
    }
}
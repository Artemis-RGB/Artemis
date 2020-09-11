using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.Core;
using Artemis.UI.Shared.Services;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.LayerProperties.Timeline
{
    public class TimelinePropertyViewModel<T> : Conductor<TimelineKeyframeViewModel<T>>.Collection.AllActive, ITimelinePropertyViewModel
    {
        private readonly IProfileEditorService _profileEditorService;
        public LayerProperty<T> LayerProperty { get; }
        public LayerPropertyViewModel LayerPropertyViewModel { get; }

        public TimelinePropertyViewModel(LayerProperty<T> layerProperty, LayerPropertyViewModel layerPropertyViewModel, IProfileEditorService profileEditorService)
        {
            _profileEditorService = profileEditorService;

            LayerProperty = layerProperty;
            LayerPropertyViewModel = layerPropertyViewModel;
        }

        public List<TimeSpan> GetAllKeyframePositions()
        {
            return LayerProperty.Keyframes.Select(k => k.Position).ToList();
        }

        private void UpdateKeyframes()
        {
            // Only show keyframes if they are enabled
            if (LayerProperty.KeyframesEnabled)
            {
                var keyframes = LayerProperty.Keyframes.ToList();
                var toRemove = Items.Where(t => !keyframes.Contains(t.LayerPropertyKeyframe)).ToList();
                foreach (var timelineKeyframeViewModel in toRemove)
                    timelineKeyframeViewModel.Dispose();

                Items.RemoveRange(toRemove);
                Items.AddRange(keyframes
                    .Where(k => Items.All(t => t.LayerPropertyKeyframe != k))
                    .Select(k => new TimelineKeyframeViewModel<T>(k, _profileEditorService))
                );
            }
            else
                Items.Clear();

            foreach (var timelineKeyframeViewModel in Items)
                timelineKeyframeViewModel.Update();
        }

        public void Dispose()
        {
        }
    }

    public interface ITimelinePropertyViewModel : IScreen, IDisposable
    {
        List<TimeSpan> GetAllKeyframePositions();
    }
}
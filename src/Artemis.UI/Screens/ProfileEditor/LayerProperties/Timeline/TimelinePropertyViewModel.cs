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
        private double _width;

        public TimelinePropertyViewModel(LayerProperty<T> layerProperty, LayerPropertyViewModel layerPropertyViewModel, IProfileEditorService profileEditorService)
        {
            _profileEditorService = profileEditorService;

            LayerProperty = layerProperty;
            LayerPropertyViewModel = layerPropertyViewModel;

            LayerProperty.KeyframesToggled += LayerPropertyOnKeyframesToggled;
            LayerProperty.KeyframeAdded += LayerPropertyOnKeyframeAdded;
            LayerProperty.KeyframeRemoved += LayerPropertyOnKeyframeRemoved;
            UpdateKeyframes();
        }

        public LayerProperty<T> LayerProperty { get; }
        public LayerPropertyViewModel LayerPropertyViewModel { get; }

        public double Width
        {
            get => _width;
            set => SetAndNotify(ref _width, value);
        }

        public List<ITimelineKeyframeViewModel> GetAllKeyframeViewModels()
        {
            return Items.Cast<ITimelineKeyframeViewModel>().ToList();
        }

        public void WipeKeyframes(TimeSpan? start, TimeSpan? end)
        {
            start ??= TimeSpan.Zero;
            end ??= TimeSpan.MaxValue;

            List<LayerPropertyKeyframe<T>> toShift = LayerProperty.Keyframes.Where(k => k.Position >= start && k.Position < end).ToList();
            foreach (LayerPropertyKeyframe<T> keyframe in toShift)
                LayerProperty.RemoveKeyframe(keyframe);

            UpdateKeyframes();
        }

        public void ShiftKeyframes(TimeSpan? start, TimeSpan? end, TimeSpan amount)
        {
            start ??= TimeSpan.Zero;
            end ??= TimeSpan.MaxValue;

            List<LayerPropertyKeyframe<T>> toShift = LayerProperty.Keyframes.Where(k => k.Position > start && k.Position < end).ToList();
            foreach (LayerPropertyKeyframe<T> keyframe in toShift)
                keyframe.Position += amount;
            
            UpdateKeyframes();
        }

        public void Dispose()
        {
            LayerProperty.KeyframesToggled -= LayerPropertyOnKeyframesToggled;
            LayerProperty.KeyframeAdded -= LayerPropertyOnKeyframeAdded;
            LayerProperty.KeyframeRemoved -= LayerPropertyOnKeyframeRemoved;
        }

        private void LayerPropertyOnKeyframesToggled(object sender, LayerPropertyEventArgs<T> e)
        {
            UpdateKeyframes();
        }

        private void LayerPropertyOnKeyframeRemoved(object sender, LayerPropertyEventArgs<T> e)
        {
            UpdateKeyframes();
        }

        private void LayerPropertyOnKeyframeAdded(object sender, LayerPropertyEventArgs<T> e)
        {
            UpdateKeyframes();
        }

        private void UpdateKeyframes()
        {
            // Only show keyframes if they are enabled
            if (LayerProperty.KeyframesEnabled)
            {
                List<LayerPropertyKeyframe<T>> keyframes = LayerProperty.Keyframes.ToList();
                List<TimelineKeyframeViewModel<T>> toRemove = Items.Where(t => !keyframes.Contains(t.LayerPropertyKeyframe)).ToList();
                foreach (TimelineKeyframeViewModel<T> timelineKeyframeViewModel in toRemove)
                    timelineKeyframeViewModel.Dispose();

                Items.RemoveRange(toRemove);
                Items.AddRange(keyframes
                    .Where(k => Items.All(t => t.LayerPropertyKeyframe != k))
                    .Select(k => new TimelineKeyframeViewModel<T>(k, _profileEditorService))
                );
            }
            else
                Items.Clear();

            foreach (TimelineKeyframeViewModel<T> timelineKeyframeViewModel in Items)
                timelineKeyframeViewModel.Update();
        }
    }

    public interface ITimelinePropertyViewModel : IScreen, IDisposable
    {
        List<ITimelineKeyframeViewModel> GetAllKeyframeViewModels();
        void WipeKeyframes(TimeSpan? start, TimeSpan? end);
        void ShiftKeyframes(TimeSpan? start, TimeSpan? end, TimeSpan amount);
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.Core;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.LayerProperties
{
    public class LayerPropertyTimelineViewModel<T> : Screen, ILayerPropertyTimelineViewModel
    {
        public LayerProperty<T> LayerProperty { get; }
        public LayerPropertyViewModel LayerPropertyViewModel { get; }

        public LayerPropertyTimelineViewModel(LayerProperty<T> layerProperty, LayerPropertyViewModel layerPropertyViewModel)
        {
            LayerProperty = layerProperty;
            LayerPropertyViewModel = layerPropertyViewModel;
        }

        public List<TimeSpan> GetAllKeyframePositions()
        {
            return LayerProperty.Keyframes.Select(k => k.Position).ToList();
        }

        public void Dispose()
        {
        }
    }

    public interface ILayerPropertyTimelineViewModel : IScreen, IDisposable
    {
        List<TimeSpan> GetAllKeyframePositions();
    }
}
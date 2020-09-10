using System;
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

        public void Dispose()
        {
        }
    }

    public interface ILayerPropertyTimelineViewModel : IScreen, IDisposable
    {
    }
}
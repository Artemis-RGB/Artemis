using System;
using Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.Abstract;

namespace Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.Timeline
{
    public class TimelinePropertyViewModel<T> : TimelinePropertyViewModel
    {
        public TimelinePropertyViewModel(LayerPropertyBaseViewModel layerPropertyBaseViewModel) : base(layerPropertyBaseViewModel)
        {
            LayerPropertyViewModel = (LayerPropertyViewModel<T>) layerPropertyBaseViewModel;
        }

        public LayerPropertyViewModel<T> LayerPropertyViewModel { get; }

        public override void Dispose()
        {
        }
    }

    public abstract class TimelinePropertyViewModel : IDisposable
    {
        protected TimelinePropertyViewModel(LayerPropertyBaseViewModel layerPropertyBaseViewModel)
        {
            LayerPropertyBaseViewModel = layerPropertyBaseViewModel;
        }

        public LayerPropertyBaseViewModel LayerPropertyBaseViewModel { get; }
        public abstract void Dispose();
    }
}
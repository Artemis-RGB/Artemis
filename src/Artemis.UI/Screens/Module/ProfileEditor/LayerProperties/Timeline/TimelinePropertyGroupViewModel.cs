using System;
using System.ComponentModel;
using System.Linq;
using Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.Abstract;
using Stylet;

namespace Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.Timeline
{
    public class TimelinePropertyGroupViewModel
    {
        public TimelinePropertyGroupViewModel(LayerPropertyBaseViewModel layerPropertyBaseViewModel)
        {
            LayerPropertyGroupViewModel = (LayerPropertyGroupViewModel) layerPropertyBaseViewModel;
            TimelineKeyframeViewModels = new BindableCollection<double>();

            LayerPropertyGroupViewModel.ProfileEditorService.PixelsPerSecondChanged += ProfileEditorServiceOnPixelsPerSecondChanged;
            LayerPropertyGroupViewModel.PropertyChanged += LayerPropertyGroupViewModelOnPropertyChanged;
            UpdateKeyframes();
        }

        public LayerPropertyGroupViewModel LayerPropertyGroupViewModel { get; }
        public BindableCollection<double> TimelineKeyframeViewModels { get; set; }
        public TimelineViewModel TimelineViewModel { get; set; }

        public void UpdateKeyframes()
        {
            TimelineKeyframeViewModels.Clear();
            TimelineKeyframeViewModels.AddRange(LayerPropertyGroupViewModel.GetKeyframes(false)
                .Select(k => LayerPropertyGroupViewModel.ProfileEditorService.PixelsPerSecond * k.Position.TotalSeconds));
        }


        public void Dispose()
        {
            LayerPropertyGroupViewModel.ProfileEditorService.PixelsPerSecondChanged -= ProfileEditorServiceOnPixelsPerSecondChanged;
            LayerPropertyGroupViewModel.PropertyChanged -= LayerPropertyGroupViewModelOnPropertyChanged;
        }

        private void LayerPropertyGroupViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(LayerPropertyGroupViewModel.IsExpanded))
                UpdateKeyframes();
        }

        private void ProfileEditorServiceOnPixelsPerSecondChanged(object sender, EventArgs e)
        {
            UpdateKeyframes();
        }
    }
}
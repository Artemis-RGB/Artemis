using System;
using System.ComponentModel;
using System.Linq;
using Artemis.Core;
using Artemis.UI.Shared.Services;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.LayerProperties.Timeline
{
    public class TimelineGroupViewModel : PropertyChangedBase, IDisposable
    {
        private readonly IProfileEditorService _profileEditorService;

        public TimelineGroupViewModel(LayerPropertyGroupViewModel layerPropertyGroupViewModel, IProfileEditorService profileEditorService)
        {
            _profileEditorService = profileEditorService;

            LayerPropertyGroupViewModel = layerPropertyGroupViewModel;
            LayerPropertyGroup = LayerPropertyGroupViewModel.LayerPropertyGroup;
            KeyframePositions = new BindableCollection<double>();

            _profileEditorService.PixelsPerSecondChanged += ProfileEditorServiceOnPixelsPerSecondChanged;
            LayerPropertyGroupViewModel.PropertyChanged += LayerPropertyGroupViewModelOnPropertyChanged;
            
            UpdateKeyframePositions();
        }


        public LayerPropertyGroupViewModel LayerPropertyGroupViewModel { get; }
        public LayerPropertyGroup LayerPropertyGroup { get; }

        public BindableCollection<double> KeyframePositions { get; }

        public void UpdateKeyframePositions()
        {
            KeyframePositions.Clear();
            KeyframePositions.AddRange(LayerPropertyGroupViewModel
                .GetAllKeyframeViewModels(false)
                .Select(p => p.Position.TotalSeconds * _profileEditorService.PixelsPerSecond));
        }

        #region IDisposable

        public void Dispose()
        {
            _profileEditorService.PixelsPerSecondChanged -= ProfileEditorServiceOnPixelsPerSecondChanged;
            LayerPropertyGroupViewModel.PropertyChanged -= LayerPropertyGroupViewModelOnPropertyChanged;
        }

        #endregion
        
        #region Event handlers

        private void LayerPropertyGroupViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(LayerPropertyGroupViewModel.IsExpanded))
                UpdateKeyframePositions();
        }

        private void ProfileEditorServiceOnPixelsPerSecondChanged(object sender, EventArgs e)
        {
            UpdateKeyframePositions();
        }

        #endregion
    }
}
using System;
using System.ComponentModel;
using System.Linq;
using Artemis.Core;
using Artemis.UI.Shared.Services;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.LayerProperties.Timeline
{
    public sealed class TimelineGroupViewModel : Screen
    {
        private readonly IProfileEditorService _profileEditorService;

        public TimelineGroupViewModel(LayerPropertyGroupViewModel layerPropertyGroupViewModel, IProfileEditorService profileEditorService)
        {
            _profileEditorService = profileEditorService;

            LayerPropertyGroupViewModel = layerPropertyGroupViewModel;
            LayerPropertyGroup = LayerPropertyGroupViewModel.LayerPropertyGroup;
            KeyframePositions = new BindableCollection<double>();

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

        #region Overrides of Screen

        /// <inheritdoc />
        protected override void OnInitialActivate()
        {
            _profileEditorService.PixelsPerSecondChanged += ProfileEditorServiceOnPixelsPerSecondChanged;
            LayerPropertyGroupViewModel.PropertyChanged += LayerPropertyGroupViewModelOnPropertyChanged;
            base.OnInitialActivate();
        }

        /// <inheritdoc />
        protected override void OnClose()
        {
            _profileEditorService.PixelsPerSecondChanged -= ProfileEditorServiceOnPixelsPerSecondChanged;
            LayerPropertyGroupViewModel.PropertyChanged -= LayerPropertyGroupViewModelOnPropertyChanged;
            base.OnClose();
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
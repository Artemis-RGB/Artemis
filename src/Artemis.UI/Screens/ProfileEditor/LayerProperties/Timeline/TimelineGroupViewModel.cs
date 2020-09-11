using System;
using System.Linq;
using Artemis.Core;
using Artemis.UI.Shared.Services;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.LayerProperties.Timeline
{
    public class TimelineGroupViewModel : IDisposable
    {
        private readonly IProfileEditorService _profileEditorService;

        public TimelineGroupViewModel(LayerPropertyGroupViewModel layerPropertyGroupViewModel, IProfileEditorService profileEditorService)
        {
            _profileEditorService = profileEditorService;

            LayerPropertyGroupViewModel = layerPropertyGroupViewModel;
            LayerPropertyGroup = LayerPropertyGroupViewModel.LayerPropertyGroup;
            KeyframePositions = new BindableCollection<double>();

            _profileEditorService.PixelsPerSecondChanged += ProfileEditorServiceOnPixelsPerSecondChanged;
            UpdateKeyframePositions();
        }

        public LayerPropertyGroupViewModel LayerPropertyGroupViewModel { get; }
        public LayerPropertyGroup LayerPropertyGroup { get; }

        public BindableCollection<double> KeyframePositions { get; }

        public void Dispose()
        {
            _profileEditorService.PixelsPerSecondChanged -= ProfileEditorServiceOnPixelsPerSecondChanged;
        }

        private void ProfileEditorServiceOnPixelsPerSecondChanged(object? sender, EventArgs e)
        {
            UpdateKeyframePositions();
        }

        public void UpdateKeyframePositions()
        {
            KeyframePositions.Clear();
            KeyframePositions.AddRange(LayerPropertyGroupViewModel
                .GetAllKeyframeViewModels(false)
                .Select(p => p.Position.TotalSeconds * _profileEditorService.PixelsPerSecond));
        }
    }
}
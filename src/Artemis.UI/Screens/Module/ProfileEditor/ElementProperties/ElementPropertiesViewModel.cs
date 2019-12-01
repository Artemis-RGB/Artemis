using System;
using Artemis.Core.Plugins.LayerElement;
using Artemis.UI.Services.Interfaces;

namespace Artemis.UI.Screens.Module.ProfileEditor.ElementProperties
{
    public class ElementPropertiesViewModel : ProfileEditorPanelViewModel
    {
        private readonly IProfileEditorService _profileEditorService;

        public ElementPropertiesViewModel(IProfileEditorService profileEditorService)
        {
            _profileEditorService = profileEditorService;

            _profileEditorService.SelectedLayerElementChanged += OnSelectedLayerElementChanged;
        }

        public LayerElementViewModel LayerElementViewModel { get; set; }

        private void OnSelectedLayerElementChanged(object sender, EventArgs e)
        {
            LayerElementViewModel = _profileEditorService.SelectedLayerElement?.GetViewModel();
        }
    }
}
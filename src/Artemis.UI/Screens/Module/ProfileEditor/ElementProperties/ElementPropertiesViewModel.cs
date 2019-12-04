using System;
using System.ComponentModel;
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
            if (LayerElementViewModel?.LayerElement?.Settings != null)
                LayerElementViewModel.LayerElement.Settings.PropertyChanged -= SettingsOnPropertyChanged;

            LayerElementViewModel = _profileEditorService.SelectedLayerElement?.GetViewModel();

            if (LayerElementViewModel?.LayerElement?.Settings != null)
                LayerElementViewModel.LayerElement.Settings.PropertyChanged += SettingsOnPropertyChanged;
        }

        private void SettingsOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            _profileEditorService.UpdateSelectedProfileElement();
        }
    }
}
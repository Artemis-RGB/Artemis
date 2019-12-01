using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.Core.Models.Profile;
using Artemis.Core.Models.Profile.Abstract;
using Artemis.Core.Plugins.LayerElement;
using Artemis.UI.Screens.Module.ProfileEditor.LayerElements.Dialogs;
using Artemis.UI.Services.Interfaces;
using Stylet;

namespace Artemis.UI.Screens.Module.ProfileEditor.LayerElements
{
    public class LayerElementsViewModel : ProfileEditorPanelViewModel
    {
        private readonly IDialogService _dialogService;
        private readonly IProfileEditorService _profileEditorService;
        private LayerElementViewModel _selectedLayerElement;

        public LayerElementsViewModel(IProfileEditorService profileEditorService, IDialogService dialogService)
        {
            _profileEditorService = profileEditorService;
            _dialogService = dialogService;

            LayerElements = new BindableCollection<LayerElementViewModel>();
            SelectedProfileElement = _profileEditorService.SelectedProfileElement;
            PopulateLayerElements();

            _profileEditorService.SelectedProfileElementChanged += OnSelectedProfileElementChanged;
            _profileEditorService.SelectedLayerElementChanged += OnSelectedLayerElementChanged;
        }

        public ProfileElement SelectedProfileElement { get; private set; }
        public BindableCollection<LayerElementViewModel> LayerElements { get; set; }

        public LayerElementViewModel SelectedLayerElement
        {
            get => _selectedLayerElement;
            set
            {
                _selectedLayerElement = value;
                _profileEditorService.ChangeSelectedLayerElement(value?.LayerElement);
            }
        }

        public bool CanAddLayerElement => SelectedProfileElement is Layer;
        public bool CanDeleteSelectedLayerElement => SelectedLayerElement != null;

        private void OnSelectedLayerElementChanged(object sender, EventArgs e)
        {
            _selectedLayerElement = LayerElements.FirstOrDefault(l => l.LayerElement == _profileEditorService.SelectedLayerElement);
            NotifyOfPropertyChange(() => SelectedLayerElement);
        }

        private void OnSelectedProfileElementChanged(object sender, EventArgs e)
        {
            SelectedProfileElement = _profileEditorService.SelectedProfileElement;
            PopulateLayerElements();
        }

        private void PopulateLayerElements()
        {
            LayerElements.Clear();
            if (SelectedProfileElement is Layer layer)
            {
                foreach (var layerElement in layer.LayerElements)
                    LayerElements.Add(new LayerElementViewModel(layerElement));
            }
        }

        public async void AddLayerElement()
        {
            var result = await _dialogService.ShowDialogAt<AddLayerElementViewModel>(
                "LayerElementsDialogHost",
                new Dictionary<string, object> {{"layer", (Layer) SelectedProfileElement}}
            );

            if (result is LayerElement layerElement)
                LayerElements.Add(new LayerElementViewModel(layerElement));
        }

        public async void DeleteSelectedLayerElement()
        {
            if (SelectedLayerElement == null)
                return;

            var result = await _dialogService.ShowConfirmDialogAt(
                "LayerElementsDialogHost",
                "Delete layer element",
                "Are you sure you want to delete the selected layer element?"
            );
            if (!result)
                return;
        }
    }
}
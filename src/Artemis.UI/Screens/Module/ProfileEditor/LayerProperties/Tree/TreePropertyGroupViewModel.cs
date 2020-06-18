using System.Collections.Generic;
using System.Diagnostics;
using Artemis.Core.Services.Interfaces;
using Artemis.UI.Screens.Module.ProfileEditor.Dialogs;
using Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.Abstract;
using Artemis.UI.Shared.Services.Interfaces;

namespace Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.Tree
{
    public class TreePropertyGroupViewModel
    {
        private readonly IDialogService _dialogService;
        private readonly ILayerService _layerService;
        private readonly IProfileEditorService _profileEditorService;

        public TreePropertyGroupViewModel(LayerPropertyBaseViewModel layerPropertyBaseViewModel,
            IProfileEditorService profileEditorService, ILayerService layerService, IDialogService dialogService)
        {
            _profileEditorService = profileEditorService;
            _layerService = layerService;
            _dialogService = dialogService;
            LayerPropertyGroupViewModel = (LayerPropertyGroupViewModel) layerPropertyBaseViewModel;
        }

        public LayerPropertyGroupViewModel LayerPropertyGroupViewModel { get; }

        public async void RenameEffect()
        {
            var result = await _dialogService.ShowDialogAt<RenameViewModel>(
                "PropertyTreeDialogHost",
                new Dictionary<string, object>
                {
                    {"subject", "effect"},
                    {"currentName", LayerPropertyGroupViewModel.LayerPropertyGroup.LayerEffect.Name}
                }
            );
            if (result is string newName)
            {
                LayerPropertyGroupViewModel.LayerPropertyGroup.LayerEffect.Name = newName;
                LayerPropertyGroupViewModel.LayerPropertyGroup.LayerEffect.HasBeenRenamed = true;
                _profileEditorService.UpdateSelectedProfile();
            }
        }

        public void DeleteEffect()
        {
            _layerService.RemoveLayerEffect(LayerPropertyGroupViewModel.LayerPropertyGroup.LayerEffect);
            _profileEditorService.UpdateSelectedProfile();
        }

        public void EnableToggled()
        {
            _profileEditorService.UpdateSelectedProfile();
        }
    }
}
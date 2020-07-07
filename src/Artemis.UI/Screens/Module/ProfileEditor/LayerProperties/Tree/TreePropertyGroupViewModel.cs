using System;
using System.Collections.Generic;
using System.Diagnostics;
using Artemis.Core.Services.Interfaces;
using Artemis.UI.Screens.Module.ProfileEditor.Dialogs;
using Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.Abstract;
using Artemis.UI.Shared.Services.Interfaces;
using Stylet;

namespace Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.Tree
{
    public class TreePropertyGroupViewModel : PropertyChangedBase
    {
        private readonly IDialogService _dialogService;
        private readonly IWindowManager _windowManager;
        private readonly ILayerService _layerService;
        private readonly IProfileEditorService _profileEditorService;

        public TreePropertyGroupViewModel(LayerPropertyBaseViewModel layerPropertyBaseViewModel,
            IProfileEditorService profileEditorService, ILayerService layerService, IDialogService dialogService, IWindowManager windowManager)
        {
            _profileEditorService = profileEditorService;
            _layerService = layerService;
            _dialogService = dialogService;
            _windowManager = windowManager;
            LayerPropertyGroupViewModel = (LayerPropertyGroupViewModel) layerPropertyBaseViewModel;
        }

        public LayerPropertyGroupViewModel LayerPropertyGroupViewModel { get; }

        public async void OpenBrushSettings()
        {
            try
            {
                var configurationViewModel = LayerPropertyGroupViewModel.LayerPropertyGroup.LayerBrush.GetConfigurationViewModel();
                if (configurationViewModel != null)
                    _windowManager.ShowDialog(new LayerBrushSettingsWindowViewModel(configurationViewModel));
            }
            catch (Exception e)
            {
                await _dialogService.ShowExceptionDialog("An exception occured while trying to show the brush's settings window", e);
                throw;
            }
        }

        public async void OpenEffectSettings()
        {
            try
            {
                var configurationViewModel = LayerPropertyGroupViewModel.LayerPropertyGroup.LayerEffect.GetConfigurationViewModel();
                if (configurationViewModel != null)
                    _windowManager.ShowDialog(new LayerEffectSettingsWindowViewModel(configurationViewModel));
            }
            catch (Exception e)
            {
                await _dialogService.ShowExceptionDialog("An exception occured while trying to show the effect's settings window", e);
                throw;
            }
        }

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
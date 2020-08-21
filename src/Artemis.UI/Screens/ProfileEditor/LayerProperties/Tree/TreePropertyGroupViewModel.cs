using System;
using System.Collections.Generic;
using Artemis.Core.Plugins.LayerBrushes;
using Artemis.Core.Plugins.LayerEffects;
using Artemis.Core.Services.Interfaces;
using Artemis.UI.Screens.ProfileEditor.Dialogs;
using Artemis.UI.Screens.ProfileEditor.LayerProperties.Abstract;
using Artemis.UI.Shared.Services.Interfaces;
using Ninject;
using Ninject.Parameters;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.LayerProperties.Tree
{
    public class TreePropertyGroupViewModel : PropertyChangedBase
    {
        private readonly IDialogService _dialogService;
        private readonly IWindowManager _windowManager;
        private readonly IKernel _kernel;
        private readonly IRenderElementService _renderElementService;
        private readonly IProfileEditorService _profileEditorService;

        public TreePropertyGroupViewModel(LayerPropertyBaseViewModel layerPropertyBaseViewModel,
            IProfileEditorService profileEditorService,
            IRenderElementService renderElementService,
            IDialogService dialogService,
            IWindowManager windowManager,
            IKernel kernel)
        {
            _profileEditorService = profileEditorService;
            _renderElementService = renderElementService;
            _dialogService = dialogService;
            _windowManager = windowManager;
            _kernel = kernel;
            LayerPropertyGroupViewModel = (LayerPropertyGroupViewModel) layerPropertyBaseViewModel;
        }

        public LayerPropertyGroupViewModel LayerPropertyGroupViewModel { get; }

        public void OpenBrushSettings()
        {
            var configurationViewModel = LayerPropertyGroupViewModel.LayerPropertyGroup.LayerBrush.ConfigurationDialog;
            if (configurationViewModel == null)
                return;

            try
            {
                var layerBrush = new ConstructorArgument("layerBrush", LayerPropertyGroupViewModel.LayerPropertyGroup.LayerBrush);
                var viewModel = (BrushConfigurationViewModel) _kernel.Get(configurationViewModel.Type, layerBrush);
                _windowManager.ShowDialog(new LayerBrushSettingsWindowViewModel(viewModel));
            }
            catch (Exception e)
            {
                _dialogService.ShowExceptionDialog("An exception occured while trying to show the brush's settings window", e);
                throw;
            }
        }

        public void OpenEffectSettings()
        {
            var configurationViewModel = LayerPropertyGroupViewModel.LayerPropertyGroup.LayerEffect.ConfigurationDialog;
            if (configurationViewModel == null)
                return;

            try
            {
                var layerEffect = new ConstructorArgument("layerEffect", LayerPropertyGroupViewModel.LayerPropertyGroup.LayerEffect);
                var viewModel = (EffectConfigurationViewModel) _kernel.Get(configurationViewModel.Type, layerEffect);
                _windowManager.ShowDialog(new LayerEffectSettingsWindowViewModel(viewModel));
            }
            catch (Exception e)
            {
                _dialogService.ShowExceptionDialog("An exception occured while trying to show the effect's settings window", e);
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
            _renderElementService.RemoveLayerEffect(LayerPropertyGroupViewModel.LayerPropertyGroup.LayerEffect);
            _profileEditorService.UpdateSelectedProfile();
        }

        public void EnableToggled()
        {
            _profileEditorService.UpdateSelectedProfile();
        }
    }
}
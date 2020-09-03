using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Artemis.Core.LayerBrushes;
using Artemis.Core.LayerEffects;
using Artemis.Core.Services;
using Artemis.UI.Exceptions;
using Artemis.UI.Screens.ProfileEditor.Dialogs;
using Artemis.UI.Screens.ProfileEditor.LayerProperties.Abstract;
using Artemis.UI.Shared.Services;
using Ninject;
using Ninject.Parameters;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.LayerProperties.Tree
{
    public class TreePropertyGroupViewModel : PropertyChangedBase
    {
        private readonly IDialogService _dialogService;
        private readonly IKernel _kernel;
        private readonly IProfileEditorService _profileEditorService;
        private readonly IRenderElementService _renderElementService;
        private readonly IWindowManager _windowManager;

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
            var layerBrush = LayerPropertyGroupViewModel.LayerPropertyGroup.LayerBrush;
            var configurationViewModel = layerBrush.ConfigurationDialog;
            if (configurationViewModel == null)
                return;

            try
            {
                // Limit to one constructor, there's no need to have more and it complicates things anyway
                var constructors = configurationViewModel.Type.GetConstructors();
                if (constructors.Length != 1)
                    throw new ArtemisUIException("Brush configuration dialogs must have exactly one constructor");

                // Find the BaseLayerBrush parameter, it is required by the base constructor so its there for sure
                var brushParameter = constructors.First().GetParameters().First(p => typeof(BaseLayerBrush).IsAssignableFrom(p.ParameterType));
                var argument = new ConstructorArgument(brushParameter.Name, layerBrush);
                var viewModel = (BrushConfigurationViewModel) _kernel.Get(configurationViewModel.Type, argument);

                _windowManager.ShowDialog(new LayerBrushSettingsWindowViewModel(viewModel));
            }
            catch (Exception e)
            {
                _dialogService.ShowExceptionDialog("An exception occured while trying to show the brush's settings window", e);
            }
        }

        public void OpenEffectSettings()
        {
            var layerEffect = LayerPropertyGroupViewModel.LayerPropertyGroup.LayerEffect;
            var configurationViewModel = layerEffect.ConfigurationDialog;
            if (configurationViewModel == null)
                return;

            try
            {
                // Limit to one constructor, there's no need to have more and it complicates things anyway
                var constructors = configurationViewModel.Type.GetConstructors();
                if (constructors.Length != 1)
                    throw new ArtemisUIException("Effect configuration dialogs must have exactly one constructor");

                var effectParameter = constructors.First().GetParameters().First(p => typeof(BaseLayerEffect).IsAssignableFrom(p.ParameterType));
                var argument = new ConstructorArgument(effectParameter.Name, layerEffect);
                var viewModel = (EffectConfigurationViewModel) _kernel.Get(configurationViewModel.Type, argument);
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
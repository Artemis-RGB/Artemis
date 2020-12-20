using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Artemis.Core;
using Artemis.Core.LayerBrushes;
using Artemis.Core.LayerEffects;
using Artemis.UI.Exceptions;
using Artemis.UI.Screens.ProfileEditor.Dialogs;
using Artemis.UI.Screens.ProfileEditor.Windows;
using Artemis.UI.Shared.LayerBrushes;
using Artemis.UI.Shared.LayerEffects;
using Artemis.UI.Shared.Services;
using Ninject;
using Ninject.Parameters;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.LayerProperties.Tree
{
    public class TreeGroupViewModel : Screen
    {
        public enum LayerPropertyGroupType
        {
            General,
            Transform,
            LayerBrushRoot,
            LayerEffectRoot,
            None
        }

        private readonly IDialogService _dialogService;
        private readonly IProfileEditorService _profileEditorService;
        private readonly IWindowManager _windowManager;

        public TreeGroupViewModel(LayerPropertyGroupViewModel layerPropertyGroupViewModel, IProfileEditorService profileEditorService, IDialogService dialogService, IWindowManager windowManager)
        {
            _profileEditorService = profileEditorService;
            _dialogService = dialogService;
            _windowManager = windowManager;

            LayerPropertyGroupViewModel = layerPropertyGroupViewModel;
            LayerPropertyGroup = LayerPropertyGroupViewModel.LayerPropertyGroup;

            DetermineGroupType();
        }

        public LayerPropertyGroupViewModel LayerPropertyGroupViewModel { get; }
        public LayerPropertyGroup LayerPropertyGroup { get; }
        public LayerPropertyGroupType GroupType { get; set; }

        public IObservableCollection<Screen> Items => LayerPropertyGroupViewModel.IsExpanded &&
                                                      LayerPropertyGroupViewModel.IsVisible
            ? LayerPropertyGroupViewModel.Items
            : null;

        public void OpenBrushSettings()
        {
            BaseLayerBrush layerBrush = LayerPropertyGroup.LayerBrush;
            LayerBrushConfigurationDialog configurationViewModel = (LayerBrushConfigurationDialog) layerBrush.ConfigurationDialog;
            if (configurationViewModel == null)
                return;

            try
            {
                // Limit to one constructor, there's no need to have more and it complicates things anyway
                ConstructorInfo[] constructors = configurationViewModel.Type.GetConstructors();
                if (constructors.Length != 1)
                    throw new ArtemisUIException("Brush configuration dialogs must have exactly one constructor");

                // Find the BaseLayerBrush parameter, it is required by the base constructor so its there for sure
                ParameterInfo brushParameter = constructors.First().GetParameters().First(p => typeof(BaseLayerBrush).IsAssignableFrom(p.ParameterType));
                ConstructorArgument argument = new(brushParameter.Name, layerBrush);
                BrushConfigurationViewModel viewModel = (BrushConfigurationViewModel) layerBrush.Descriptor.Provider.Plugin.Kernel.Get(configurationViewModel.Type, argument);

                _windowManager.ShowDialog(new LayerBrushSettingsWindowViewModel(viewModel));
            }
            catch (Exception e)
            {
                _dialogService.ShowExceptionDialog("An exception occured while trying to show the brush's settings window", e);
            }
        }

        public void OpenEffectSettings()
        {
            BaseLayerEffect layerEffect = LayerPropertyGroup.LayerEffect;
            LayerEffectConfigurationDialog configurationViewModel = (LayerEffectConfigurationDialog) layerEffect.ConfigurationDialog;
            if (configurationViewModel == null)
                return;

            try
            {
                // Limit to one constructor, there's no need to have more and it complicates things anyway
                ConstructorInfo[] constructors = configurationViewModel.Type.GetConstructors();
                if (constructors.Length != 1)
                    throw new ArtemisUIException("Effect configuration dialogs must have exactly one constructor");

                ParameterInfo effectParameter = constructors.First().GetParameters().First(p => typeof(BaseLayerEffect).IsAssignableFrom(p.ParameterType));
                ConstructorArgument argument = new(effectParameter.Name, layerEffect);
                EffectConfigurationViewModel viewModel = (EffectConfigurationViewModel) layerEffect.Descriptor.Provider.Plugin.Kernel.Get(configurationViewModel.Type, argument);
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
            object result = await _dialogService.ShowDialogAt<RenameViewModel>(
                "PropertyTreeDialogHost",
                new Dictionary<string, object>
                {
                    {"subject", "effect"},
                    {"currentName", LayerPropertyGroup.LayerEffect.Name}
                }
            );
            if (result is string newName)
            {
                LayerPropertyGroup.LayerEffect.Name = newName;
                LayerPropertyGroup.LayerEffect.HasBeenRenamed = true;
                _profileEditorService.UpdateSelectedProfile();
            }
        }

        public void DeleteEffect()
        {
            if (LayerPropertyGroup.LayerEffect == null)
                return;

            LayerPropertyGroup.ProfileElement.RemoveLayerEffect(LayerPropertyGroup.LayerEffect);
            _profileEditorService.UpdateSelectedProfile();
        }

        public void EnableToggled()
        {
            _profileEditorService.UpdateSelectedProfile();
        }

        public double GetDepth()
        {
            int depth = 0;
            LayerPropertyGroup current = LayerPropertyGroup.Parent;
            while (current != null)
            {
                depth++;
                current = current.Parent;
            }

            return depth;
        }

        private void LayerPropertyGroupViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(LayerPropertyGroupViewModel.IsExpanded) || e.PropertyName == nameof(LayerPropertyGroupViewModel.IsVisible))
                NotifyOfPropertyChange(nameof(Items));
        }

        private void DetermineGroupType()
        {
            if (LayerPropertyGroup is LayerGeneralProperties)
                GroupType = LayerPropertyGroupType.General;
            else if (LayerPropertyGroup is LayerTransformProperties)
                GroupType = LayerPropertyGroupType.Transform;
            else if (LayerPropertyGroup.Parent == null && LayerPropertyGroup.LayerBrush != null)
                GroupType = LayerPropertyGroupType.LayerBrushRoot;
            else if (LayerPropertyGroup.Parent == null && LayerPropertyGroup.LayerEffect != null)
                GroupType = LayerPropertyGroupType.LayerEffectRoot;
            else
                GroupType = LayerPropertyGroupType.None;
        }

        #region Overrides of Screen

        /// <inheritdoc />
        protected override void OnInitialActivate()
        {
            LayerPropertyGroupViewModel.PropertyChanged += LayerPropertyGroupViewModelOnPropertyChanged;
            base.OnInitialActivate();
        }

        /// <inheritdoc />
        protected override void OnClose()
        {
            LayerPropertyGroupViewModel.PropertyChanged -= LayerPropertyGroupViewModelOnPropertyChanged;
            base.OnClose();
        }

        #endregion
    }
}
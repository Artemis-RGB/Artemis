using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Exceptions;
using Artemis.UI.Screens.ProfileEditor.LayerProperties.DataBindings.DirectDataBinding.ModifierTypes;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Input;
using Artemis.UI.Shared.Services;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.LayerProperties.DataBindings.DirectDataBinding
{
    public sealed class DataBindingModifierViewModel<TLayerProperty, TProperty> : PropertyChangedBase, IDisposable
    {
        private readonly IDataBindingService _dataBindingService;
        private readonly IDataModelUIService _dataModelUIService;
        private readonly IProfileEditorService _profileEditorService;
        private DataModelDynamicViewModel _dynamicSelectionViewModel;
        private ModifierTypeCategoryViewModel _modifierTypeViewModels;
        private BaseDataBindingModifierType _selectedModifierType;
        private DataModelStaticViewModel _staticInputViewModel;

        public DataBindingModifierViewModel(DataBindingModifier<TLayerProperty, TProperty> modifier,
            IDataBindingService dataBindingService,
            ISettingsService settingsService,
            IDataModelUIService dataModelUIService,
            IProfileEditorService profileEditorService)
        {
            _dataBindingService = dataBindingService;
            _dataModelUIService = dataModelUIService;
            _profileEditorService = profileEditorService;

            ShowDataModelValues = settingsService.GetSetting<bool>("ProfileEditor.ShowDataModelValues");

            Modifier = modifier;
            SelectModifierTypeCommand = new DelegateCommand(ExecuteSelectModifierTypeCommand);

            Update();
        }

        public DelegateCommand SelectModifierTypeCommand { get; }
        public PluginSetting<bool> ShowDataModelValues { get; }
        public DataBindingModifier<TLayerProperty, TProperty> Modifier { get; }

        public ModifierTypeCategoryViewModel ModifierTypeViewModels
        {
            get => _modifierTypeViewModels;
            set => SetAndNotify(ref _modifierTypeViewModels, value);
        }

        public BaseDataBindingModifierType SelectedModifierType
        {
            get => _selectedModifierType;
            set => SetAndNotify(ref _selectedModifierType, value);
        }

        public DataModelDynamicViewModel DynamicSelectionViewModel
        {
            get => _dynamicSelectionViewModel;
            private set => SetAndNotify(ref _dynamicSelectionViewModel, value);
        }

        public DataModelStaticViewModel StaticInputViewModel
        {
            get => _staticInputViewModel;
            private set => SetAndNotify(ref _staticInputViewModel, value);
        }

        public void Delete()
        {
            Modifier.DirectDataBinding.RemoveModifier(Modifier);
            _profileEditorService.SaveSelectedProfileElement();
        }

        private void ParameterSelectionViewModelOnPropertySelected(object sender, DataModelInputDynamicEventArgs e)
        {
            Modifier.UpdateParameterDynamic(e.DataModelPath);
            _profileEditorService.SaveSelectedProfileElement();
        }

        private void StaticInputViewModelOnValueUpdated(object sender, DataModelInputStaticEventArgs e)
        {
            Modifier.UpdateParameterStatic(e.Value);
            _profileEditorService.SaveSelectedProfileElement();
        }

        private void Update()
        {
            Type sourceType = Modifier.DirectDataBinding.GetSourceType();
            if (sourceType == null)
                throw new ArtemisUIException("Cannot use a data binding modifier VM for a data binding without a source");

            if (Modifier.ModifierType == null || Modifier.ModifierType.ParameterType == null)
            {
                DisposeDynamicSelectionViewModel();
                DisposeStaticInputViewModel();
            }
            else if (Modifier.ParameterType == ProfileRightSideType.Dynamic)
            {
                DisposeStaticInputViewModel();
                DynamicSelectionViewModel = _dataModelUIService.GetDynamicSelectionViewModel(_profileEditorService.SelectedProfileConfiguration.Modules);
                if (DynamicSelectionViewModel != null)
                {
                    DynamicSelectionViewModel.DisplaySwitchButton = true;
                    DynamicSelectionViewModel.PropertySelected += ParameterSelectionViewModelOnPropertySelected;
                    DynamicSelectionViewModel.SwitchToStaticRequested += DynamicSelectionViewModelOnSwitchToStaticRequested;
                    DynamicSelectionViewModel.FilterTypes = new[] {Modifier.ModifierType.ParameterType ?? sourceType};
                }
            }
            else
            {
                DisposeDynamicSelectionViewModel();
                StaticInputViewModel = _dataModelUIService.GetStaticInputViewModel(Modifier.ModifierType.ParameterType ?? sourceType, null);
                if (StaticInputViewModel != null)
                {
                    StaticInputViewModel.DisplaySwitchButton = true;
                    StaticInputViewModel.ValueUpdated += StaticInputViewModelOnValueUpdated;
                    StaticInputViewModel.SwitchToDynamicRequested += StaticInputViewModelOnSwitchToDynamicRequested;
                }
            }

            // Modifier type
            ModifierTypeCategoryViewModel root = new(null, null);
            IEnumerable<IGrouping<string, BaseDataBindingModifierType>> modifierTypes = _dataBindingService.GetCompatibleModifierTypes(sourceType, ModifierTypePart.Value).GroupBy(t => t.Category);
            foreach (IGrouping<string, BaseDataBindingModifierType> dataBindingModifierTypes in modifierTypes)
            {
                IEnumerable<ModifierTypeViewModel> viewModels = dataBindingModifierTypes.Select(t => new ModifierTypeViewModel(t));
                if (dataBindingModifierTypes.Key == null)
                    root.Children.AddRange(viewModels);
                else
                    root.Children.Add(new ModifierTypeCategoryViewModel(dataBindingModifierTypes.Key, viewModels));
            }

            ModifierTypeViewModels = root;
            SelectedModifierType = Modifier.ModifierType;

            // Parameter
            if (DynamicSelectionViewModel != null)
                DynamicSelectionViewModel.ChangeDataModelPath(Modifier.ParameterPath);
            else if (StaticInputViewModel != null) 
                StaticInputViewModel.Value = Modifier.ParameterStaticValue;
        }

        private void ExecuteSelectModifierTypeCommand(object context)
        {
            if (!(context is ModifierTypeViewModel modifierTypeViewModel))
                return;

            Modifier.UpdateModifierType(modifierTypeViewModel.ModifierType);
            _profileEditorService.SaveSelectedProfileElement();

            Update();
        }

        #region IDisposable

        /// <inheritdoc />
        public void Dispose()
        {
            DisposeDynamicSelectionViewModel();
            DisposeStaticInputViewModel();
        }

        private void DisposeStaticInputViewModel()
        {
            if (StaticInputViewModel != null)
            {
                StaticInputViewModel.Dispose();
                StaticInputViewModel.ValueUpdated -= StaticInputViewModelOnValueUpdated;
                StaticInputViewModel.SwitchToDynamicRequested -= StaticInputViewModelOnSwitchToDynamicRequested;
                StaticInputViewModel = null;
            }
        }

        private void DisposeDynamicSelectionViewModel()
        {
            if (DynamicSelectionViewModel != null)
            {
                DynamicSelectionViewModel.Dispose();
                DynamicSelectionViewModel.PropertySelected -= ParameterSelectionViewModelOnPropertySelected;
                DynamicSelectionViewModel.SwitchToStaticRequested -= DynamicSelectionViewModelOnSwitchToStaticRequested;
                DynamicSelectionViewModel = null;
            }
        }

        #endregion

        #region Event handlers

        private void DynamicSelectionViewModelOnSwitchToStaticRequested(object sender, EventArgs e)
        {
            Modifier.ParameterType = ProfileRightSideType.Static;

            // Ensure the right static value is never null when the preferred type is a value type
            if (SelectedModifierType.ParameterType != null && 
                SelectedModifierType.ParameterType.IsValueType && Modifier.ParameterStaticValue == null)
                Modifier.UpdateParameterStatic(SelectedModifierType.ParameterType.GetDefault());

            Update();
        }

        private void StaticInputViewModelOnSwitchToDynamicRequested(object sender, EventArgs e)
        {
            Modifier.ParameterType = ProfileRightSideType.Dynamic;
            Update();
        }

        #endregion
    }
}
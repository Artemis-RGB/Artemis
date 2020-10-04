using System;
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
    public class DataBindingModifierViewModel<TLayerProperty, TProperty> : PropertyChangedBase, IDisposable
    {
        private readonly IDataBindingService _dataBindingService;
        private readonly IDataModelUIService _dataModelUIService;
        private readonly IProfileEditorService _profileEditorService;
        private DataModelDynamicViewModel _dynamicSelectionViewModel;
        private ModifierTypeCategoryViewModel _modifierTypeViewModels;
        private DataBindingModifierType _selectedModifierType;
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

        public DataBindingModifierType SelectedModifierType
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

        public void Dispose()
        {
            if (DynamicSelectionViewModel != null)
            {
                DynamicSelectionViewModel.Dispose();
                DynamicSelectionViewModel.PropertySelected -= ParameterSelectionViewModelOnPropertySelected;
            }

            if (StaticInputViewModel != null)
            {
                StaticInputViewModel.Dispose();
                StaticInputViewModel.ValueUpdated -= StaticInputViewModelOnValueUpdated;
            }
        }

        public void Delete()
        {
            Modifier.DirectDataBinding.RemoveModifier(Modifier);
            _profileEditorService.UpdateSelectedProfileElement();
        }

        public void SwapType()
        {
            if (Modifier.ParameterType == ProfileRightSideType.Dynamic)
            {
                var sourceType = Modifier.DirectDataBinding.GetSourceType();
                Modifier.UpdateParameter((Modifier.ModifierType.ParameterType ?? sourceType).GetDefault());
            }
            else
                Modifier.UpdateParameter(null, null);

            Update();
            _profileEditorService.UpdateSelectedProfileElement();
        }

        private void ParameterSelectionViewModelOnPropertySelected(object sender, DataModelInputDynamicEventArgs e)
        {
            Modifier.UpdateParameter(e.DataModelVisualizationViewModel.DataModel, e.DataModelVisualizationViewModel.Path);
            _profileEditorService.UpdateSelectedProfileElement();
        }

        private void StaticInputViewModelOnValueUpdated(object sender, DataModelInputStaticEventArgs e)
        {
            Modifier.UpdateParameter(e.Value);
            _profileEditorService.UpdateSelectedProfileElement();
        }

        private void Update()
        {
            var sourceType = Modifier.DirectDataBinding.GetSourceType();
            if (sourceType == null)
                throw new ArtemisUIException("Cannot use a data binding modifier VM for a data binding without a source");

            if (DynamicSelectionViewModel != null)
            {
                DynamicSelectionViewModel.Dispose();
                DynamicSelectionViewModel.PropertySelected -= ParameterSelectionViewModelOnPropertySelected;
            }

            if (Modifier.ModifierType == null || !Modifier.ModifierType.SupportsParameter)
            {
                StaticInputViewModel = null;
                DynamicSelectionViewModel = null;
            }
            else if (Modifier.ParameterType == ProfileRightSideType.Dynamic)
            {
                StaticInputViewModel = null;
                DynamicSelectionViewModel = _dataModelUIService.GetDynamicSelectionViewModel(_profileEditorService.GetCurrentModule());
                if (DynamicSelectionViewModel != null)
                {
                    DynamicSelectionViewModel.PropertySelected += ParameterSelectionViewModelOnPropertySelected;
                    DynamicSelectionViewModel.FilterTypes = new[] {Modifier.ModifierType.ParameterType ?? sourceType};
                }
            }
            else
            {
                DynamicSelectionViewModel = null;
                StaticInputViewModel = _dataModelUIService.GetStaticInputViewModel(Modifier.ModifierType.ParameterType ?? sourceType);
                if (StaticInputViewModel != null)
                    StaticInputViewModel.ValueUpdated += StaticInputViewModelOnValueUpdated;
            }

            // Modifier type
            var root = new ModifierTypeCategoryViewModel(null, null);
            var modifierTypes = _dataBindingService.GetCompatibleModifierTypes(sourceType).GroupBy(t => t.Category);
            foreach (var dataBindingModifierTypes in modifierTypes)
            {
                var viewModels = dataBindingModifierTypes.Select(t => new ModifierTypeViewModel(t));
                if (dataBindingModifierTypes.Key == null)
                    root.Children.AddRange(viewModels);
                else
                    root.Children.Add(new ModifierTypeCategoryViewModel(dataBindingModifierTypes.Key, viewModels));
            }

            ModifierTypeViewModels = root;
            SelectedModifierType = Modifier.ModifierType;

            // Parameter
            if (DynamicSelectionViewModel != null)
                DynamicSelectionViewModel.PopulateSelectedPropertyViewModel(Modifier.ParameterDataModel, Modifier.ParameterPropertyPath);
            else if (StaticInputViewModel != null)
                StaticInputViewModel.Value = Modifier.ParameterStaticValue;
        }

        private void ExecuteSelectModifierTypeCommand(object context)
        {
            if (!(context is ModifierTypeViewModel modifierTypeViewModel))
                return;

            Modifier.UpdateModifierType(modifierTypeViewModel.ModifierType);
            _profileEditorService.UpdateSelectedProfileElement();

            Update();
        }
    }
}
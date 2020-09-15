using System;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Exceptions;
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
            ModifierTypes = new BindableCollection<DataBindingModifierType>();

            SelectModifierTypeCommand = new DelegateCommand(ExecuteSelectModifierTypeCommand);

            Update();
        }

        public DelegateCommand SelectModifierTypeCommand { get; }
        public PluginSetting<bool> ShowDataModelValues { get; }
        public DataBindingModifier<TLayerProperty, TProperty> Modifier { get; }
        public BindableCollection<DataBindingModifierType> ModifierTypes { get; }

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

        public void Delete()
        {
            Modifier.DirectDataBinding.RemoveModifier(Modifier);
        }

        public void SwapType()
        {
            if (Modifier.ParameterType == ProfileRightSideType.Dynamic)
                Modifier.UpdateParameter(Modifier.DirectDataBinding.GetSourceType().GetDefault());
            else
                Modifier.UpdateParameter(null, null);

            Update();
        }

        private void ParameterSelectionViewModelOnPropertySelected(object sender, DataModelInputDynamicEventArgs e)
        {
            Modifier.UpdateParameter(e.DataModelVisualizationViewModel.DataModel, e.DataModelVisualizationViewModel.PropertyPath);
        }

        private void StaticInputViewModelOnValueUpdated(object sender, DataModelInputStaticEventArgs e)
        {
            Modifier.UpdateParameter(e.Value);
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
                    DynamicSelectionViewModel.FilterTypes = new[] {sourceType};
                }
            }
            else
            {
                DynamicSelectionViewModel = null;
                if (Modifier.ModifierType.PreferredParameterType != null && sourceType.IsCastableFrom(Modifier.ModifierType.PreferredParameterType))
                    StaticInputViewModel = _dataModelUIService.GetStaticInputViewModel(Modifier.ModifierType.PreferredParameterType);
                else
                    StaticInputViewModel = _dataModelUIService.GetStaticInputViewModel(sourceType);

                if (StaticInputViewModel != null)
                    StaticInputViewModel.ValueUpdated += StaticInputViewModelOnValueUpdated;
            }

            // Modifier type
            ModifierTypes.Clear();
            ModifierTypes.AddRange(_dataBindingService.GetCompatibleModifierTypes(sourceType));
            SelectedModifierType = Modifier.ModifierType;

            // Parameter
            if (DynamicSelectionViewModel != null)
                DynamicSelectionViewModel.PopulateSelectedPropertyViewModel(Modifier.ParameterDataModel, Modifier.ParameterPropertyPath);
            else if (StaticInputViewModel != null)
                StaticInputViewModel.Value = Modifier.ParameterStaticValue;
        }

        private void ExecuteSelectModifierTypeCommand(object context)
        {
            if (!(context is DataBindingModifierType dataBindingModifierType))
                return;

            Modifier.UpdateModifierType(dataBindingModifierType);
            _profileEditorService.UpdateSelectedProfileElement();

            Update();
        }

        public void Dispose()
        {
            if (DynamicSelectionViewModel != null)
            {
                DynamicSelectionViewModel.Dispose();
                DynamicSelectionViewModel.PropertySelected -= ParameterSelectionViewModelOnPropertySelected;
            }
        }
    }
}
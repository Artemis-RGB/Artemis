using System;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Exceptions;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Input;
using Artemis.UI.Shared.Services;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.LayerProperties.DataBindings
{
    public class DataBindingModifierViewModel : PropertyChangedBase
    {
        private readonly IDataBindingService _dataBindingService;
        private readonly IDataModelUIService _dataModelUIService;
        private readonly IProfileEditorService _profileEditorService;
        private DataBindingModifierType _selectedModifierType;
        private DataModelDynamicViewModel _dynamicSelectionViewModel;
        private DataModelStaticViewModel _staticInputViewModel;

        public DataBindingModifierViewModel(DataBindingModifier modifier,
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

            // Initialize async, no need to wait for it
            Execute.PostToUIThread(Initialize);
        }

        public DelegateCommand SelectModifierTypeCommand { get; }
        public PluginSetting<bool> ShowDataModelValues { get; }
        public DataBindingModifier Modifier { get; }
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

        private void Initialize()
        {
            var sourceType = Modifier.DataBinding.GetSourceType();
            if (sourceType == null)
                throw new ArtemisUIException("Cannot initialize a data binding modifier VM for a data binding without a source");

            if (Modifier.ParameterType == ProfileRightSideType.Dynamic)
            {
                StaticInputViewModel = null;
                DynamicSelectionViewModel = _dataModelUIService.GetDynamicSelectionViewModel(_profileEditorService.GetCurrentModule());
                DynamicSelectionViewModel.PropertySelected += ParameterSelectionViewModelOnPropertySelected;
                DynamicSelectionViewModel.FilterTypes = new[] {sourceType};
            }
            else
            {
                DynamicSelectionViewModel = null;
                StaticInputViewModel = _dataModelUIService.GetStaticInputViewModel(sourceType);
                StaticInputViewModel.ValueUpdated += StaticInputViewModelOnValueUpdated;
            }

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
            var sourceType = Modifier.DataBinding.GetSourceType();

            // Modifier type
            ModifierTypes.Clear();
            ModifierTypes.AddRange(_dataBindingService.GetCompatibleModifierTypes(sourceType));
            SelectedModifierType = Modifier.ModifierType;

            // Parameter
            if (DynamicSelectionViewModel != null)
                DynamicSelectionViewModel?.PopulateSelectedPropertyViewModel(Modifier.ParameterDataModel, Modifier.ParameterPropertyPath);
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

        public void Delete()
        {
            Modifier.DataBinding.RemoveModifier(Modifier);
        }

        public void SwapType()
        {
            if (Modifier.ParameterType == ProfileRightSideType.Dynamic)
                Modifier.UpdateParameter(Modifier.DataBinding.GetSourceType().GetDefault());
            else
                Modifier.UpdateParameter(null, null);

            Initialize();
        }
    }
}
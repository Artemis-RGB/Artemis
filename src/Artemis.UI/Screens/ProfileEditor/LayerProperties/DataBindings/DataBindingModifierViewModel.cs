using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Shared;
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
            Task.Run(Initialize);
        }

        public DelegateCommand SelectModifierTypeCommand { get; set; }

        public PluginSetting<bool> ShowDataModelValues { get; }
        public DataBindingModifier Modifier { get; }
        public BindableCollection<DataBindingModifierType> ModifierTypes { get; }

        public DataBindingModifierType SelectedModifierType
        {
            get => _selectedModifierType;
            set => SetAndNotify(ref _selectedModifierType, value);
        }

        public DataModelSelectionViewModel ParameterSelectionViewModel { get; private set; }

        private void Initialize()
        {
            ParameterSelectionViewModel = _dataModelUIService.GetDataModelSelectionViewModel(_profileEditorService.GetCurrentModule());
            ParameterSelectionViewModel.PropertySelected += ParameterSelectionViewModelOnPropertySelected;
            ParameterSelectionViewModel.FilterTypes = new[] {Modifier.DataBinding.TargetProperty.PropertyType};

            Update();
        }

        private void ParameterSelectionViewModelOnPropertySelected(object sender, DataModelPropertySelectedEventArgs e)
        {
            Modifier.UpdateParameter(e.DataModelVisualizationViewModel.DataModel, e.DataModelVisualizationViewModel.PropertyPath);
        }

        private void Update()
        {
            // Modifier type
            ModifierTypes.Clear();
            ModifierTypes.AddRange(_dataBindingService.GetCompatibleModifierTypes(Modifier.DataBinding.TargetProperty.PropertyType));
            SelectedModifierType = Modifier.ModifierType;

            ParameterSelectionViewModel.PopulateSelectedPropertyViewModel(Modifier.ParameterDataModel, Modifier.ParameterPropertyPath);
        }

        private void ExecuteSelectModifierTypeCommand(object context)
        {
            if (!(context is DataBindingModifierType dataBindingModifierType))
                return;

            Modifier.UpdateModifierType(dataBindingModifierType);
            _profileEditorService.UpdateSelectedProfileElement();

            Update();
        }
    }
}
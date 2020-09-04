using System;
using System.Threading.Tasks;
using System.Timers;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using Artemis.UI.Utilities;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.LayerProperties.DataBindings
{
    public class DataBindingModifierViewModel : PropertyChangedBase
    {
        private readonly IDataBindingService _dataBindingService;
        private readonly IDataModelUIService _dataModelUIService;
        private readonly IProfileEditorService _profileEditorService;
        private readonly Timer _updateTimer;
        private DataModelPropertiesViewModel _parameterDataModel;
        private bool _parameterDataModelOpen;
        private DataBindingModifierType _selectedModifierType;
        private DataModelVisualizationViewModel _selectedParameterProperty;

        public DataBindingModifierViewModel(DataBindingModifier modifier,
            IDataBindingService dataBindingService,
            ISettingsService settingsService,
            IDataModelUIService dataModelUIService,
            IProfileEditorService profileEditorService)
        {
            _dataBindingService = dataBindingService;
            _dataModelUIService = dataModelUIService;
            _profileEditorService = profileEditorService;
            _updateTimer = new Timer(500);

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

        public DataModelPropertiesViewModel ParameterDataModel
        {
            get => _parameterDataModel;
            set => SetAndNotify(ref _parameterDataModel, value);
        }

        public bool ParameterDataModelOpen
        {
            get => _parameterDataModelOpen;
            set => SetAndNotify(ref _parameterDataModelOpen, value);
        }

        public DataModelVisualizationViewModel SelectedParameterProperty
        {
            get => _selectedParameterProperty;
            set => SetAndNotify(ref _selectedParameterProperty, value);
        }

        private void Initialize()
        {
            // Get the data models
            ParameterDataModel = _dataModelUIService.GetMainDataModelVisualization();
            if (!_dataModelUIService.GetPluginExtendsDataModel(_profileEditorService.GetCurrentModule()))
                ParameterDataModel.Children.Add(_dataModelUIService.GetPluginDataModelVisualization(_profileEditorService.GetCurrentModule()));

            ParameterDataModel.UpdateRequested += ParameterDataModelOnUpdateRequested;

            Update();

            _updateTimer.Start();
            _updateTimer.Elapsed += OnUpdateTimerOnElapsed;
        }

        private void Update()
        {
            // Modifier type
            ModifierTypes.Clear();
            ModifierTypes.AddRange(_dataBindingService.GetCompatibleModifierTypes(Modifier.DataBinding.TargetProperty.PropertyType));
            SelectedModifierType = Modifier.ModifierType;

            // Determine the parameter property
            if (Modifier.ParameterDataModel == null)
                SelectedParameterProperty = null;
            else
                SelectedParameterProperty = ParameterDataModel.GetChildByPath(Modifier.ParameterDataModel.PluginInfo.Guid, Modifier.ParameterPropertyPath);
        }

        private void ExecuteSelectModifierTypeCommand(object context)
        {
            if (!(context is DataBindingModifierType dataBindingModifierType))
                return;

            Modifier.UpdateModifierType(dataBindingModifierType);
            _profileEditorService.UpdateSelectedProfileElement();

            Update();
        }

        private void ParameterDataModelOnUpdateRequested(object sender, EventArgs e)
        {
            ParameterDataModel.ApplyTypeFilter(true, Modifier.DataBinding.TargetProperty.PropertyType);
        }

        private void OnUpdateTimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            if (ParameterDataModelOpen)
                ParameterDataModel.Update(_dataModelUIService);
        }
    }
}
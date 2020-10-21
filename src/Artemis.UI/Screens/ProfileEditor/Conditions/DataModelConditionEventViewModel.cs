using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using Artemis.Core;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Screens.ProfileEditor.Conditions.Abstract;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Input;
using Artemis.UI.Shared.Services;

namespace Artemis.UI.Screens.ProfileEditor.Conditions
{
    public class DataModelConditionEventViewModel : DataModelConditionViewModel, IDisposable
    {
        private readonly IDataModelConditionsVmFactory _dataModelConditionsVmFactory;
        private readonly IDataModelUIService _dataModelUIService;
        private readonly IProfileEditorService _profileEditorService;
        private DataModelDynamicViewModel _targetSelectionViewModel;

        public DataModelConditionEventViewModel(DataModelConditionEvent model,
            IProfileEditorService profileEditorService,
            IDataModelUIService dataModelUIService,
            IDataModelConditionsVmFactory dataModelConditionsVmFactory) : base(model)
        {
            _profileEditorService = profileEditorService;
            _dataModelUIService = dataModelUIService;
            _dataModelConditionsVmFactory = dataModelConditionsVmFactory;

            Initialize();
        }

        public DataModelConditionEvent DataModelConditionEvent => (DataModelConditionEvent) Model;

        public DataModelDynamicViewModel TargetSelectionViewModel
        {
            get => _targetSelectionViewModel;
            set => SetAndNotify(ref _targetSelectionViewModel, value);
        }

        public void Initialize()
        {
            TargetSelectionViewModel = _dataModelUIService.GetDynamicSelectionViewModel(_profileEditorService.GetCurrentModule());
            TargetSelectionViewModel.PropertySelected += TargetSelectionViewModelOnPropertySelected;

            IReadOnlyCollection<DataModelVisualizationRegistration> editors = _dataModelUIService.RegisteredDataModelEditors;
            List<Type> supportedInputTypes = editors.Select(e => e.SupportedType).ToList();
            supportedInputTypes.AddRange(editors.Where(e => e.CompatibleConversionTypes != null).SelectMany(e => e.CompatibleConversionTypes));
            supportedInputTypes.Add(typeof(IEnumerable<>));
            TargetSelectionViewModel.FilterTypes = supportedInputTypes.ToArray();

            TargetSelectionViewModel.ButtonBrush = new SolidColorBrush(Color.FromRgb(188, 174, 71));
            TargetSelectionViewModel.Placeholder = "Select a list";

            Update();
        }

        public void ApplyEvent()
        {
            if (!TargetSelectionViewModel.DataModelPath.GetPropertyType().IsGenericEnumerable())
            {
                if (Parent is DataModelConditionGroupViewModel groupViewModel)
                    groupViewModel.ConvertToPredicate(this);
                return;
            }

            DataModelConditionEvent.UpdateEvent(TargetSelectionViewModel.DataModelPath);
            _profileEditorService.UpdateSelectedProfileElement();

            Update();
        }

        #region Event handlers

        private void TargetSelectionViewModelOnPropertySelected(object? sender, DataModelInputDynamicEventArgs e)
        {
            ApplyEvent();
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            TargetSelectionViewModel.Dispose();
            TargetSelectionViewModel.PropertySelected -= TargetSelectionViewModelOnPropertySelected;
        }

        #endregion
    }
}
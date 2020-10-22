using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Artemis.Core;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Screens.ProfileEditor.Conditions.Abstract;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;

namespace Artemis.UI.Screens.ProfileEditor.Conditions
{
    public class DataModelConditionEventViewModel : DataModelConditionViewModel, IDisposable
    {
        private readonly IDataModelConditionsVmFactory _dataModelConditionsVmFactory;
        private readonly IDataModelUIService _dataModelUIService;
        private readonly IProfileEditorService _profileEditorService;

        public DataModelConditionEventViewModel(DataModelConditionEvent dataModelConditionEvent,
            IProfileEditorService profileEditorService,
            IDataModelUIService dataModelUIService,
            IDataModelConditionsVmFactory dataModelConditionsVmFactory) : base(dataModelConditionEvent)
        {
            _profileEditorService = profileEditorService;
            _dataModelUIService = dataModelUIService;
            _dataModelConditionsVmFactory = dataModelConditionsVmFactory;

            Initialize();
        }

        public DataModelConditionEvent DataModelConditionEvent => (DataModelConditionEvent) Model;


        public void Initialize()
        {
            LeftSideSelectionViewModel = _dataModelUIService.GetDynamicSelectionViewModel(_profileEditorService.GetCurrentModule());
            LeftSideSelectionViewModel.PropertySelected += LeftSideSelectionViewModelOnPropertySelected;

            IReadOnlyCollection<DataModelVisualizationRegistration> editors = _dataModelUIService.RegisteredDataModelEditors;
            List<Type> supportedInputTypes = editors.Select(e => e.SupportedType).ToList();
            supportedInputTypes.AddRange(editors.Where(e => e.CompatibleConversionTypes != null).SelectMany(e => e.CompatibleConversionTypes));
            supportedInputTypes.Add(typeof(IEnumerable<>));
            LeftSideSelectionViewModel.FilterTypes = supportedInputTypes.ToArray();

            LeftSideSelectionViewModel.ButtonBrush = new SolidColorBrush(Color.FromRgb(185, 164, 10));
            LeftSideSelectionViewModel.Placeholder = "Select an event";

            Update();
        }

        public override void Update()
        {
            LeftSideSelectionViewModel.ChangeDataModelPath(DataModelConditionEvent.EventPath);
        }

        public void ApplyEvent()
        {
            Type newType = LeftSideSelectionViewModel.DataModelPath.GetPropertyType();
            bool converted = ConvertIfRequired(newType);
            if (converted)
                return;

            DataModelConditionEvent.UpdateEvent(LeftSideSelectionViewModel.DataModelPath);
            _profileEditorService.UpdateSelectedProfileElement();

            Update();
        }

        #region Event handlers

        private void LeftSideSelectionViewModelOnPropertySelected(object? sender, DataModelInputDynamicEventArgs e)
        {
            ApplyEvent();
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            LeftSideSelectionViewModel.Dispose();
            LeftSideSelectionViewModel.PropertySelected -= LeftSideSelectionViewModelOnPropertySelected;
        }

        #endregion
    }
}
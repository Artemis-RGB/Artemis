using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;

namespace Artemis.UI.Screens.ProfileEditor.Conditions
{
    public class DataModelConditionEventPredicateViewModel : DataModelConditionPredicateViewModel
    {
        private readonly IDataModelUIService _dataModelUIService;
        private readonly IProfileEditorService _profileEditorService;

        public DataModelConditionEventPredicateViewModel(DataModelConditionEventPredicate dataModelConditionEventPredicate,
            IProfileEditorService profileEditorService,
            IDataModelUIService dataModelUIService,
            IConditionOperatorService conditionOperatorService,
            ISettingsService settingsService)
            : base(dataModelConditionEventPredicate, profileEditorService, dataModelUIService, conditionOperatorService, settingsService)
        {
            _profileEditorService = profileEditorService;
            _dataModelUIService = dataModelUIService;

            LeftSideColor = new SolidColorBrush(Color.FromRgb(185, 164, 10));
            Initialize();
        }

        public DataModelConditionEventPredicate DataModelConditionEventPredicate => (DataModelConditionEventPredicate) Model;

        public override void Initialize()
        {
            base.Initialize();

            DataModelPropertiesViewModel eventDataModel = GetEventDataModel();
            LeftSideSelectionViewModel.ChangeDataModel(eventDataModel);
        }

        protected override List<Type> GetSupportedInputTypes()
        {
            IReadOnlyCollection<DataModelVisualizationRegistration> editors = _dataModelUIService.RegisteredDataModelEditors;
            List<Type> supportedInputTypes = editors.Select(e => e.SupportedType).ToList();
            supportedInputTypes.AddRange(editors.Where(e => e.CompatibleConversionTypes != null).SelectMany(e => e.CompatibleConversionTypes));

            return supportedInputTypes;
        }

        protected override Type GetLeftSideType()
        {
            return LeftSideSelectionViewModel.DataModelPath?.GetPropertyType();
        }

        protected override List<DataModelPropertiesViewModel> GetExtraRightSideDataModelViewModels()
        {
            return new List<DataModelPropertiesViewModel> {GetEventDataModel()};
        }

        private DataModelPropertiesViewModel GetEventDataModel()
        {
            EventPredicateWrapperDataModel wrapper = EventPredicateWrapperDataModel.Create(
                DataModelConditionEventPredicate.DataModelConditionEvent.EventArgumentType
            );

            return wrapper.CreateViewModel();
        }
    }
}
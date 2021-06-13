using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Artemis.Core;
using Artemis.Core.Modules;
using Artemis.Core.Services;
using Artemis.UI.Extensions;
using Artemis.UI.Screens.ProfileEditor.Conditions.Abstract;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.Conditions
{
    public class DataModelConditionEventPredicateViewModel : DataModelConditionPredicateViewModel
    {
        private readonly IDataModelUIService _dataModelUIService;

        public DataModelConditionEventPredicateViewModel(DataModelConditionEventPredicate dataModelConditionEventPredicate,
            List<Module> modules,
            IProfileEditorService profileEditorService,
            IDataModelUIService dataModelUIService,
            IConditionOperatorService conditionOperatorService,
            ISettingsService settingsService)
            : base(dataModelConditionEventPredicate, modules, profileEditorService, dataModelUIService, conditionOperatorService, settingsService)
        {
            _dataModelUIService = dataModelUIService;

            LeftSideColor = new SolidColorBrush(Color.FromRgb(185, 164, 10));
        }

        public DataModelConditionEventPredicate DataModelConditionEventPredicate => (DataModelConditionEventPredicate) Model;

        public override void Initialize()
        {
            base.Initialize();

            DataModelPropertiesViewModel eventDataModel = GetEventDataModel();
            LeftSideSelectionViewModel.ChangeDataModel(eventDataModel);
        }

        protected override void OnInitialActivate()
        {
            base.OnInitialActivate();
            Initialize();
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
            // Only the VM housing the event arguments is expected here which is a DataModelPropertiesViewModel
            return GetEventDataModel().Children.Cast<DataModelPropertiesViewModel>().ToList();
        }

        private DataModelPropertiesViewModel GetEventDataModel()
        {
            EventPredicateWrapperDataModel wrapper = EventPredicateWrapperDataModel.Create(
                DataModelConditionEventPredicate.DataModelConditionEvent.EventArgumentType
            );

            return wrapper.CreateViewModel(_dataModelUIService, new DataModelUpdateConfiguration(false));
        }

        public override void Evaluate()
        {
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;

namespace Artemis.UI.Screens.ProfileEditor.Conditions
{
    public class DataModelConditionGeneralPredicateViewModel : DataModelConditionPredicateViewModel
    {
        private readonly IDataModelUIService _dataModelUIService;

        public DataModelConditionGeneralPredicateViewModel(DataModelConditionGeneralPredicate dataModelConditionGeneralPredicate,
            IProfileEditorService profileEditorService,
            IDataModelUIService dataModelUIService,
            IConditionOperatorService conditionOperatorService,
            ISettingsService settingsService)
            : base(dataModelConditionGeneralPredicate, profileEditorService, dataModelUIService, conditionOperatorService, settingsService)
        {
            _dataModelUIService = dataModelUIService;
            Initialize();
        }

        protected override List<Type> GetSupportedInputTypes()
        {
            IReadOnlyCollection<DataModelVisualizationRegistration> editors = _dataModelUIService.RegisteredDataModelEditors;
            List<Type> supportedInputTypes = editors.Select(e => e.SupportedType).ToList();
            supportedInputTypes.AddRange(editors.Where(e => e.CompatibleConversionTypes != null).SelectMany(e => e.CompatibleConversionTypes));
            supportedInputTypes.Add(typeof(IEnumerable<>));
            supportedInputTypes.Add(typeof(DataModelEvent));
            supportedInputTypes.Add(typeof(DataModelEvent<>));

            return supportedInputTypes;
        }

        protected override Type GetLeftSideType()
        {
            return LeftSideSelectionViewModel.DataModelPath?.GetPropertyType();
        }
    }
}
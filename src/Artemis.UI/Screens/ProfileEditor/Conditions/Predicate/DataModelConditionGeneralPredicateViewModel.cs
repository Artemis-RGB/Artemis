﻿using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.Core;
using Artemis.Core.Modules;
using Artemis.Core.Services;
using Artemis.UI.Screens.ProfileEditor.Conditions.Abstract;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;

namespace Artemis.UI.Screens.ProfileEditor.Conditions
{
    public class DataModelConditionGeneralPredicateViewModel : DataModelConditionPredicateViewModel
    {
        private readonly IDataModelUIService _dataModelUIService;

        public DataModelConditionGeneralPredicateViewModel(DataModelConditionGeneralPredicate dataModelConditionGeneralPredicate,
            List<Module> modules,
            IProfileEditorService profileEditorService,
            IDataModelUIService dataModelUIService,
            IConditionOperatorService conditionOperatorService,
            ISettingsService settingsService)
            : base(dataModelConditionGeneralPredicate, modules, profileEditorService, dataModelUIService, conditionOperatorService, settingsService)
        {
            _dataModelUIService = dataModelUIService;
        }

        protected override void OnInitialActivate()
        {
            Initialize();
        }

        protected override List<Type> GetSupportedInputTypes()
        {
            IReadOnlyCollection<DataModelVisualizationRegistration> editors = _dataModelUIService.RegisteredDataModelEditors;
            List<Type> supportedInputTypes = editors.Select(e => e.SupportedType).ToList();
            supportedInputTypes.AddRange(editors.Where(e => e.CompatibleConversionTypes != null).SelectMany(e => e.CompatibleConversionTypes));
            supportedInputTypes.Add(typeof(IEnumerable<>));

            return supportedInputTypes;
        }

        protected override Type GetLeftSideType()
        {
            return LeftSideSelectionViewModel.DataModelPath?.GetPropertyType();
        }

        public override void Evaluate()
        {
            IsConditionMet = DataModelConditionPredicate.Evaluate();
        }
    }
}
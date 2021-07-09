using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Artemis.Core;
using Artemis.Core.Modules;
using Artemis.Core.Services;
using Artemis.UI.Screens.ProfileEditor.Conditions.Abstract;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;

namespace Artemis.UI.Screens.ProfileEditor.Conditions
{
    public class DataModelConditionListPredicateViewModel : DataModelConditionPredicateViewModel
    {
        private readonly IDataModelUIService _dataModelUIService;

        public DataModelConditionListPredicateViewModel(DataModelConditionListPredicate dataModelConditionListPredicate,
            List<Module> modules,
            IProfileEditorService profileEditorService,
            IDataModelUIService dataModelUIService,
            IConditionOperatorService conditionOperatorService,
            ISettingsService settingsService)
            : base(dataModelConditionListPredicate, modules, profileEditorService, dataModelUIService, conditionOperatorService, settingsService)
        {
            _dataModelUIService = dataModelUIService;
            LeftSideColor = new SolidColorBrush(Color.FromRgb(71, 108, 188));
        }

        public DataModelConditionListPredicate DataModelConditionListPredicate => (DataModelConditionListPredicate) Model;

        public override void Initialize()
        {
            base.Initialize();

            DataModelPropertiesViewModel listDataModel = GetListDataModel();
            LeftSideSelectionViewModel.ChangeDataModel(listDataModel);

            // If this is a primitive list the user doesn't have much to choose, so preselect the list item for them
            if (DataModelConditionListPredicate.DataModelConditionList.IsPrimitiveList && DataModelConditionListPredicate.LeftPath == null)
            {
                DataModelConditionListPredicate.UpdateLeftSide(listDataModel.Children.FirstOrDefault()?.DataModelPath);
                Update();
            }
        }

        public override void Evaluate()
        {
            
        }

        public override void UpdateModules()
        {
            foreach (DataModelConditionViewModel dataModelConditionViewModel in Items) 
                dataModelConditionViewModel.UpdateModules();
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
            return DataModelConditionListPredicate.DataModelConditionList.IsPrimitiveList
                ? DataModelConditionListPredicate.DataModelConditionList.ListType
                : LeftSideSelectionViewModel.DataModelPath?.GetPropertyType();
        }

        protected override List<DataModelPropertiesViewModel> GetExtraRightSideDataModelViewModels()
        {
            if (GetListDataModel()?.Children?.FirstOrDefault() is DataModelPropertiesViewModel listValue)
                return new List<DataModelPropertiesViewModel> {listValue};
            return null;
        }

        private DataModelPropertiesViewModel GetListDataModel()
        {
            ListPredicateWrapperDataModel wrapper = ListPredicateWrapperDataModel.Create(
                DataModelConditionListPredicate.DataModelConditionList.ListType!,
                DataModelConditionListPredicate.DataModelConditionList.ListPath?.GetPropertyDescription()?.ListItemName
            );

            return wrapper.CreateViewModel(_dataModelUIService, new DataModelUpdateConfiguration(true));
        }
    }
}
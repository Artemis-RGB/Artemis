using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Exceptions;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;

namespace Artemis.UI.Screens.ProfileEditor.Conditions
{
    public class DataModelConditionListPredicateViewModel : DataModelConditionPredicateViewModel
    {
        private readonly IDataModelUIService _dataModelUIService;
        private readonly IProfileEditorService _profileEditorService;
        private bool _isPrimitiveList;

        public DataModelConditionListPredicateViewModel(DataModelConditionListPredicate dataModelConditionListPredicate,
            IProfileEditorService profileEditorService,
            IDataModelUIService dataModelUIService,
            IConditionOperatorService conditionOperatorService,
            ISettingsService settingsService)
            : base(dataModelConditionListPredicate, profileEditorService, dataModelUIService, conditionOperatorService, settingsService)
        {
            _profileEditorService = profileEditorService;
            _dataModelUIService = dataModelUIService;

            LeftSideColor = new SolidColorBrush(Color.FromRgb(71, 108, 188));
            Initialize();
        }

        public DataModelConditionListPredicate DataModelConditionListPredicate => (DataModelConditionListPredicate) Model;

        public override void Initialize()
        {
            base.Initialize();

            DataModelPropertiesViewModel listDataModel = GetListDataModel();
            if (listDataModel.Children.Count == 1 && listDataModel.Children.First() is DataModelListPropertyViewModel)
                _isPrimitiveList = true;
            else
                _isPrimitiveList = false;

            // Get the data models
            if (!_isPrimitiveList)
                LeftSideSelectionViewModel.ChangeDataModel(listDataModel);
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
            if (DataModelConditionListPredicate.DataModelConditionList.ListPath?.DataModelGuid == null)
                throw new ArtemisUIException("Failed to retrieve the list data model VM for this list predicate because it has no list path");

            DataModelPropertiesViewModel dataModel = _dataModelUIService.GetPluginDataModelVisualization(_profileEditorService.GetCurrentModule(), true);
            DataModelListViewModel listDataModel = (DataModelListViewModel) dataModel.GetChildByPath(
                DataModelConditionListPredicate.DataModelConditionList.ListPath.DataModelGuid.Value,
                DataModelConditionListPredicate.DataModelConditionList.ListPath.Path
            );

            return listDataModel.GetListTypeViewModel(_dataModelUIService);
        }
    }
}
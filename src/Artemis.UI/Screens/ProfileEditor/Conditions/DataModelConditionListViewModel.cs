using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Artemis.Core;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Screens.ProfileEditor.Conditions.Abstract;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using Humanizer;

namespace Artemis.UI.Screens.ProfileEditor.Conditions
{
    public sealed class DataModelConditionListViewModel : DataModelConditionViewModel, IDisposable
    {
        private readonly IDataModelConditionsVmFactory _dataModelConditionsVmFactory;
        private readonly IDataModelUIService _dataModelUIService;
        private readonly IProfileEditorService _profileEditorService;

        public DataModelConditionListViewModel(
            DataModelConditionList dataModelConditionList,
            IProfileEditorService profileEditorService,
            IDataModelUIService dataModelUIService,
            IDataModelConditionsVmFactory dataModelConditionsVmFactory) : base(dataModelConditionList)
        {
            _profileEditorService = profileEditorService;
            _dataModelUIService = dataModelUIService;
            _dataModelConditionsVmFactory = dataModelConditionsVmFactory;
        }

        public DataModelConditionList DataModelConditionList => (DataModelConditionList) Model;

        public string SelectedListOperator => DataModelConditionList.ListOperator.Humanize();

        public void SelectListOperator(string type)
        {
            ListOperator enumValue = Enum.Parse<ListOperator>(type);
            DataModelConditionList.ListOperator = enumValue;
            NotifyOfPropertyChange(nameof(SelectedListOperator));

            _profileEditorService.UpdateSelectedProfileElement();
        }

        public void AddCondition()
        {
            DataModelConditionList.AddChild(new DataModelConditionGeneralPredicate(DataModelConditionList, ProfileRightSideType.Dynamic));

            Update();
            _profileEditorService.UpdateSelectedProfileElement();
        }

        public void AddGroup()
        {
            DataModelConditionList.AddChild(new DataModelConditionGroup(DataModelConditionList));

            Update();
            _profileEditorService.UpdateSelectedProfileElement();
        }

        public override void Delete()
        {
            base.Delete();
            _profileEditorService.UpdateSelectedProfileElement();
        }

        public void Initialize()
        {
            LeftSideSelectionViewModel = _dataModelUIService.GetDynamicSelectionViewModel(_profileEditorService.GetCurrentModule());
            LeftSideSelectionViewModel.PropertySelected += LeftSideSelectionViewModelOnPropertySelected;

            IReadOnlyCollection<DataModelVisualizationRegistration> editors = _dataModelUIService.RegisteredDataModelEditors;
            List<Type> supportedInputTypes = editors.Select(e => e.SupportedType).ToList();
            supportedInputTypes.AddRange(editors.Where(e => e.CompatibleConversionTypes != null).SelectMany(e => e.CompatibleConversionTypes));
            supportedInputTypes.Add(typeof(IEnumerable<>));

            LeftSideSelectionViewModel.FilterTypes = supportedInputTypes.ToArray();
            LeftSideSelectionViewModel.ButtonBrush = new SolidColorBrush(Color.FromRgb(71, 108, 188));
            LeftSideSelectionViewModel.Placeholder = "Select a list";

            Update();
        }

        public void ApplyList()
        {
            Type newType = LeftSideSelectionViewModel.DataModelPath.GetPropertyType();
            bool converted = ConvertIfRequired(newType);
            if (converted)
                return;

            DataModelConditionList.UpdateList(LeftSideSelectionViewModel.DataModelPath);
            _profileEditorService.UpdateSelectedProfileElement();

            Update();
        }

        public override void Update()
        {
            LeftSideSelectionViewModel.ChangeDataModelPath(DataModelConditionList.ListPath);
            NotifyOfPropertyChange(nameof(SelectedListOperator));

            // Remove VMs of effects no longer applied on the layer
            Items.RemoveRange(Items.Where(c => !DataModelConditionList.Children.Contains(c.Model)).ToList());

            if (DataModelConditionList.ListPath == null || !DataModelConditionList.ListPath.IsValid)
                return;

            List<DataModelConditionViewModel> viewModels = new();
            foreach (DataModelConditionPart childModel in Model.Children)
            {
                if (Items.Any(c => c.Model == childModel))
                    continue;
                if (!(childModel is DataModelConditionGroup dataModelConditionGroup))
                    continue;

                DataModelConditionGroupViewModel viewModel = _dataModelConditionsVmFactory.DataModelConditionGroupViewModel(dataModelConditionGroup, ConditionGroupType.List);
                viewModel.IsRootGroup = true;
                viewModels.Add(viewModel);
            }

            if (viewModels.Any())
                Items.AddRange(viewModels);

            foreach (DataModelConditionViewModel childViewModel in Items)
                childViewModel.Update();
        }

        protected override void OnInitialActivate()
        {
            Initialize();
            base.OnInitialActivate();
        }

        private void LeftSideSelectionViewModelOnPropertySelected(object sender, DataModelInputDynamicEventArgs e)
        {
            ApplyList();
        }

        #region IDisposable

        public void Dispose()
        {
            LeftSideSelectionViewModel.Dispose();
            LeftSideSelectionViewModel.PropertySelected -= LeftSideSelectionViewModelOnPropertySelected;
        }

        #endregion
    }
}
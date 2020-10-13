using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Artemis.Core;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Screens.ProfileEditor.Conditions.Abstract;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Input;
using Artemis.UI.Shared.Services;
using Humanizer;

namespace Artemis.UI.Screens.ProfileEditor.Conditions
{
    public class DataModelConditionListViewModel : DataModelConditionViewModel, IDisposable
    {
        private readonly IDataModelConditionsVmFactory _dataModelConditionsVmFactory;
        private readonly IDataModelUIService _dataModelUIService;
        private readonly IProfileEditorService _profileEditorService;
        private DataModelDynamicViewModel _targetSelectionViewModel;

        public DataModelConditionListViewModel(
            DataModelConditionList dataModelConditionList,
            IProfileEditorService profileEditorService,
            IDataModelUIService dataModelUIService,
            IDataModelConditionsVmFactory dataModelConditionsVmFactory) : base(dataModelConditionList)
        {
            _profileEditorService = profileEditorService;
            _dataModelUIService = dataModelUIService;
            _dataModelConditionsVmFactory = dataModelConditionsVmFactory;

            Initialize();
        }

        public DataModelDynamicViewModel TargetSelectionViewModel
        {
            get => _targetSelectionViewModel;
            set => SetAndNotify(ref _targetSelectionViewModel, value);
        }

        public DataModelConditionList DataModelConditionList => (DataModelConditionList) Model;

        public string SelectedListOperator => DataModelConditionList.ListOperator.Humanize();

        public void Dispose()
        {
            TargetSelectionViewModel.Dispose();
            TargetSelectionViewModel.PropertySelected -= TargetSelectionViewModelOnPropertySelected;
        }

        public void SelectListOperator(string type)
        {
            ListOperator enumValue = Enum.Parse<ListOperator>(type);
            DataModelConditionList.ListOperator = enumValue;
            NotifyOfPropertyChange(nameof(SelectedListOperator));

            _profileEditorService.UpdateSelectedProfileElement();
        }

        public void AddCondition()
        {
            DataModelConditionList.AddChild(new DataModelConditionPredicate(DataModelConditionList, ProfileRightSideType.Dynamic));

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
            TargetSelectionViewModel = _dataModelUIService.GetDynamicSelectionViewModel(_profileEditorService.GetCurrentModule());
            TargetSelectionViewModel.PropertySelected += TargetSelectionViewModelOnPropertySelected;

            IReadOnlyCollection<DataModelVisualizationRegistration> editors = _dataModelUIService.RegisteredDataModelEditors;
            List<Type> supportedInputTypes = editors.Select(e => e.SupportedType).ToList();
            supportedInputTypes.AddRange(editors.Where(e => e.CompatibleConversionTypes != null).SelectMany(e => e.CompatibleConversionTypes));
            supportedInputTypes.Add(typeof(IEnumerable<>));
            TargetSelectionViewModel.FilterTypes = supportedInputTypes.ToArray();

            TargetSelectionViewModel.ButtonBrush = new SolidColorBrush(Color.FromRgb(71, 108, 188));
            TargetSelectionViewModel.Placeholder = "Select a list";

            Update();
        }

        public void ApplyList()
        {
            if (!TargetSelectionViewModel.DataModelPath.GetPropertyType().IsGenericEnumerable())
            {
                if (Parent is DataModelConditionGroupViewModel groupViewModel)
                    groupViewModel.ConvertToPredicate(this);
                return;
            }
            DataModelConditionList.UpdateList(TargetSelectionViewModel.DataModelPath);
            _profileEditorService.UpdateSelectedProfileElement();

            Update();
        }

        public override void Update()
        {
            TargetSelectionViewModel.ChangeDataModelPath(DataModelConditionList.ListPath);
            NotifyOfPropertyChange(nameof(SelectedListOperator));

            // Remove VMs of effects no longer applied on the layer
            Items.RemoveRange(Items.Where(c => !DataModelConditionList.Children.Contains(c.Model)).ToList());

            if (DataModelConditionList.ListPath == null || !DataModelConditionList.ListPath.IsValid)
                return;

            List<DataModelConditionViewModel> viewModels = new List<DataModelConditionViewModel>();
            foreach (DataModelConditionPart childModel in Model.Children)
            {
                if (Items.Any(c => c.Model == childModel))
                    continue;
                if (!(childModel is DataModelConditionGroup dataModelConditionGroup))
                    continue;

                DataModelConditionGroupViewModel viewModel = _dataModelConditionsVmFactory.DataModelConditionGroupViewModel(dataModelConditionGroup, true);
                viewModel.IsRootGroup = true;
                viewModels.Add(viewModel);
            }

            if (viewModels.Any())
                Items.AddRange(viewModels);

            foreach (DataModelConditionViewModel childViewModel in Items)
                childViewModel.Update();
        }

        private void TargetSelectionViewModelOnPropertySelected(object? sender, DataModelInputDynamicEventArgs e)
        {
            ApplyList();
        }
    }
}
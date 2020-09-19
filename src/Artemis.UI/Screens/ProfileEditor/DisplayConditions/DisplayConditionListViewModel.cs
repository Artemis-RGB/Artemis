using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Screens.ProfileEditor.DisplayConditions.Abstract;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using Artemis.UI.Utilities;
using Humanizer;

namespace Artemis.UI.Screens.ProfileEditor.DisplayConditions
{
    public class DisplayConditionListViewModel : DisplayConditionViewModel, IDisposable
    {
        private readonly IDataModelUIService _dataModelUIService;
        private readonly IDisplayConditionsVmFactory _displayConditionsVmFactory;
        private readonly IProfileEditorService _profileEditorService;
        private readonly Timer _updateTimer;
        private bool _isInitialized;
        private DataModelListViewModel _selectedListProperty;
        private DataModelPropertiesViewModel _targetDataModel;

        public DisplayConditionListViewModel(
            DataModelConditionList dataModelConditionList,
            DisplayConditionViewModel parent,
            IProfileEditorService profileEditorService,
            IDataModelUIService dataModelUIService,
            IDisplayConditionsVmFactory displayConditionsVmFactory,
            ISettingsService settingsService) : base(dataModelConditionList)
        {
            _profileEditorService = profileEditorService;
            _dataModelUIService = dataModelUIService;
            _displayConditionsVmFactory = displayConditionsVmFactory;
            _updateTimer = new Timer(500);

            SelectListPropertyCommand = new DelegateCommand(ExecuteSelectListProperty);

            ShowDataModelValues = settingsService.GetSetting<bool>("ProfileEditor.ShowDataModelValues");

            // Initialize async, no need to wait for it
            Task.Run(Initialize);
        }

        public DelegateCommand SelectListPropertyCommand { get; }
        public PluginSetting<bool> ShowDataModelValues { get; }

        public DataModelConditionList DataModelConditionList => (DataModelConditionList) Model;

        public bool IsInitialized
        {
            get => _isInitialized;
            set => SetAndNotify(ref _isInitialized, value);
        }

        public bool TargetDataModelOpen { get; set; }

        public DataModelPropertiesViewModel TargetDataModel
        {
            get => _targetDataModel;
            set => SetAndNotify(ref _targetDataModel, value);
        }

        public DataModelListViewModel SelectedListProperty
        {
            get => _selectedListProperty;
            set => SetAndNotify(ref _selectedListProperty, value);
        }

        public string SelectedListOperator => DataModelConditionList.ListOperator.Humanize();

        public void SelectListOperator(string type)
        {
            var enumValue = Enum.Parse<ListOperator>(type);
            DataModelConditionList.ListOperator = enumValue;
            NotifyOfPropertyChange(nameof(SelectedListOperator));

            _profileEditorService.UpdateSelectedProfileElement();
        }

        public void AddCondition(string type)
        {
            if (type == "Static")
                DataModelConditionList.AddChild(new DataModelConditionPredicate(DataModelConditionList, ProfileRightSideType.Static));
            else if (type == "Dynamic")
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
            // Get the data models
            TargetDataModel = _dataModelUIService.GetPluginDataModelVisualization(_profileEditorService.GetCurrentModule(), true);
            TargetDataModel.UpdateRequested += TargetDataModelUpdateRequested;

            Update();

            _updateTimer.Start();
            _updateTimer.Elapsed += OnUpdateTimerOnElapsed;

            IsInitialized = true;
        }

        public void ApplyList()
        {
            DataModelConditionList.UpdateList(SelectedListProperty.DataModel, SelectedListProperty.PropertyPath);
            _profileEditorService.UpdateSelectedProfileElement();

            Update();
        }

        public override void Update()
        {
            if (TargetDataModel == null)
                return;

            NotifyOfPropertyChange(nameof(SelectedListOperator));

            // Update the selected list property
            if (DataModelConditionList.ListDataModel != null && DataModelConditionList.ListPropertyPath != null)
            {
                var child = TargetDataModel.GetChildByPath(
                    DataModelConditionList.ListDataModel.PluginInfo.Guid,
                    DataModelConditionList.ListPropertyPath
                );
                SelectedListProperty = child as DataModelListViewModel;
            }

            // Ensure filtering is applied to include Enumerables only
            TargetDataModel.ApplyTypeFilter(true, typeof(IList));

            // Remove VMs of effects no longer applied on the layer
            var toRemove = Items.Where(c => !DataModelConditionList.Children.Contains(c.Model)).ToList();
            // Using RemoveRange breaks our lovely animations
            foreach (var displayConditionViewModel in toRemove)
                Items.Remove(displayConditionViewModel);

            foreach (var childModel in Model.Children)
            {
                if (Items.Any(c => c.Model == childModel))
                    continue;
                if (!(childModel is DataModelConditionGroup displayConditionGroup))
                    continue;

                var viewModel = _displayConditionsVmFactory.DisplayConditionGroupViewModel(displayConditionGroup, true);
                viewModel.IsRootGroup = true;
                Items.Add(viewModel);
            }

            foreach (var childViewModel in Items)
                childViewModel.Update();
        }

        private void OnUpdateTimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            if (TargetDataModelOpen)
            {
                TargetDataModel?.Update(_dataModelUIService);
                SelectedListProperty?.Update(_dataModelUIService);
            }
        }

        private void TargetDataModelUpdateRequested(object sender, EventArgs e)
        {
            TargetDataModel.ApplyTypeFilter(true, typeof(IList));
        }


        private void ExecuteSelectListProperty(object context)
        {
            if (!(context is DataModelListViewModel dataModelListViewModel))
                return;

            SelectedListProperty = dataModelListViewModel;
            ApplyList();
        }

        public void Dispose()
        {
            _updateTimer.Dispose();
            _updateTimer.Elapsed -= OnUpdateTimerOnElapsed;
        }
    }
}
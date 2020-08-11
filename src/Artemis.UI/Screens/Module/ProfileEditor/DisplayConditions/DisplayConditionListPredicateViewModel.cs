using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Artemis.Core.Models.Profile.Conditions;
using Artemis.Core.Plugins.Models;
using Artemis.Core.Services;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Screens.Module.ProfileEditor.DisplayConditions.Abstract;
using Artemis.UI.Shared.DataModelVisualization.Shared;
using Artemis.UI.Shared.Services;
using Artemis.UI.Shared.Services.Interfaces;
using Artemis.UI.Utilities;
using Humanizer;

namespace Artemis.UI.Screens.Module.ProfileEditor.DisplayConditions
{
    public class DisplayConditionListPredicateViewModel : DisplayConditionViewModel
    {
        private readonly IProfileEditorService _profileEditorService;
        private readonly IDataModelVisualizationService _dataModelVisualizationService;
        private readonly IDisplayConditionsVmFactory _displayConditionsVmFactory;
        private bool _isInitialized;
        private DataModelVisualizationViewModel _selectedListProperty;
        private DataModelPropertiesViewModel _targetDataModel;
        private readonly Timer _updateTimer;

        public DisplayConditionListPredicateViewModel(
            DisplayConditionListPredicate displayConditionListPredicate,
            DisplayConditionViewModel parent,
            IProfileEditorService profileEditorService,
            IDataModelVisualizationService dataModelVisualizationService,
            IDisplayConditionsVmFactory displayConditionsVmFactory,
            ISettingsService settingsService) : base(displayConditionListPredicate, parent)
        {
            _profileEditorService = profileEditorService;
            _dataModelVisualizationService = dataModelVisualizationService;
            _displayConditionsVmFactory = displayConditionsVmFactory;
            _updateTimer = new Timer(500);

            SelectListPropertyCommand = new DelegateCommand(ExecuteSelectListProperty);

            ShowDataModelValues = settingsService.GetSetting<bool>("ProfileEditor.ShowDataModelValues");

            // Initialize async, no need to wait for it
            Task.Run(Initialize);
        }

        public DelegateCommand SelectListPropertyCommand { get; }
        public PluginSetting<bool> ShowDataModelValues { get; }

        public DisplayConditionListPredicate DisplayConditionListPredicate => (DisplayConditionListPredicate) Model;

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

        public DataModelVisualizationViewModel SelectedListProperty
        {
            get => _selectedListProperty;
            set => SetAndNotify(ref _selectedListProperty, value);
        }

        public string SelectedListOperator => DisplayConditionListPredicate.ListOperator.Humanize();

        public void SelectListOperator(string type)
        {
            var enumValue = Enum.Parse<ListOperator>(type);
            DisplayConditionListPredicate.ListOperator = enumValue;
            NotifyOfPropertyChange(nameof(SelectedListOperator));
        }

        public void AddCondition(string type)
        {
            if (type == "Static")
                DisplayConditionListPredicate.AddChild(new DisplayConditionPredicate(DisplayConditionListPredicate, PredicateType.Static));
            else if (type == "Dynamic")
                DisplayConditionListPredicate.AddChild(new DisplayConditionPredicate(DisplayConditionListPredicate, PredicateType.Dynamic));

            Update();
            _profileEditorService.UpdateSelectedProfileElement();
        }

        public void AddGroup()
        {
            DisplayConditionListPredicate.AddChild(new DisplayConditionGroup(DisplayConditionListPredicate));

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
            TargetDataModel = _dataModelVisualizationService.GetMainDataModelVisualization();
            if (!_dataModelVisualizationService.GetPluginExtendsDataModel(_profileEditorService.GetCurrentModule()))
                TargetDataModel.Children.Add(_dataModelVisualizationService.GetPluginDataModelVisualization(_profileEditorService.GetCurrentModule()));

            TargetDataModel.UpdateRequested += TargetDataModelUpdateRequested;

            Update();

            _updateTimer.Start();
            _updateTimer.Elapsed += OnUpdateTimerOnElapsed;

            IsInitialized = true;
        }

        protected override void Dispose(bool disposing)
        {
            _updateTimer.Stop();
            _updateTimer.Elapsed -= OnUpdateTimerOnElapsed;
        }

        private void OnUpdateTimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            if (TargetDataModelOpen)
                TargetDataModel.Update(_dataModelVisualizationService);
        }

        private void TargetDataModelUpdateRequested(object sender, EventArgs e)
        {
            TargetDataModel.ApplyTypeFilter(true, typeof(IEnumerable));
        }

        public void ApplyList()
        {
            DisplayConditionListPredicate.UpdateList(SelectedListProperty.DataModel, SelectedListProperty.PropertyPath);
            _profileEditorService.UpdateSelectedProfileElement();

            Update();
        }

        public override void Update()
        {
            if (TargetDataModel == null)
                return;

            NotifyOfPropertyChange(nameof(SelectedListOperator));

            // Update the selected list property
            if (DisplayConditionListPredicate.ListDataModel != null && DisplayConditionListPredicate.ListPropertyPath != null)
                SelectedListProperty = TargetDataModel.GetChildByPath(DisplayConditionListPredicate.ListDataModel.PluginInfo.Guid, DisplayConditionListPredicate.ListPropertyPath);
            // Ensure filtering is applied to include Enumerables only
            TargetDataModel.ApplyTypeFilter(true, typeof(IEnumerable));

            // Remove VMs of effects no longer applied on the layer
            var toRemove = Children.Where(c => !DisplayConditionListPredicate.Children.Contains(c.Model)).ToList();
            // Using RemoveRange breaks our lovely animations
            foreach (var displayConditionViewModel in toRemove)
            {
                Children.Remove(displayConditionViewModel);
                displayConditionViewModel.Dispose();
            }

            foreach (var childModel in Model.Children)
            {
                if (Children.Any(c => c.Model == childModel))
                    continue;

                switch (childModel)
                {
                    case DisplayConditionGroup displayConditionGroup:
                        Children.Add(_displayConditionsVmFactory.DisplayConditionGroupViewModel(displayConditionGroup, this));
                        break;
                    case DisplayConditionListPredicate displayConditionListPredicate:
                        Children.Add(_displayConditionsVmFactory.DisplayConditionListPredicateViewModel(displayConditionListPredicate, this));
                        break;
                    case DisplayConditionPredicate displayConditionPredicate:
                        Children.Add(_displayConditionsVmFactory.DisplayConditionPredicateViewModel(displayConditionPredicate, this));
                        break;
                }
            }

            foreach (var childViewModel in Children)
                childViewModel.Update();
        }


        private void ExecuteSelectListProperty(object context)
        {
            if (!(context is DataModelVisualizationViewModel dataModelVisualizationViewModel))
                return;

            SelectedListProperty = dataModelVisualizationViewModel;
            ApplyList();
        }
    }
}
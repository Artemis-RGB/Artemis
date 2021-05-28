using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Artemis.Core;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Screens.ProfileEditor.Conditions.Abstract;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;

namespace Artemis.UI.Screens.ProfileEditor.Conditions
{
    public sealed class DataModelConditionEventViewModel : DataModelConditionViewModel, IDisposable
    {
        private readonly IDataModelConditionsVmFactory _dataModelConditionsVmFactory;
        private readonly IDataModelUIService _dataModelUIService;
        private readonly IProfileEditorService _profileEditorService;
        private DateTime _lastTrigger;
        private string _triggerPastParticiple;

        public DataModelConditionEventViewModel(DataModelConditionEvent dataModelConditionEvent,
            IProfileEditorService profileEditorService,
            IDataModelUIService dataModelUIService,
            IDataModelConditionsVmFactory dataModelConditionsVmFactory) : base(dataModelConditionEvent)
        {
            _profileEditorService = profileEditorService;
            _dataModelUIService = dataModelUIService;
            _dataModelConditionsVmFactory = dataModelConditionsVmFactory;

            _lastTrigger = DataModelConditionEvent.LastTrigger;
        }

        public DataModelConditionEvent DataModelConditionEvent => (DataModelConditionEvent) Model;

        public DateTime LastTrigger
        {
            get => _lastTrigger;
            set => SetAndNotify(ref _lastTrigger, value);
        }

        public string TriggerPastParticiple
        {
            get => _triggerPastParticiple;
            set => SetAndNotify(ref _triggerPastParticiple, value);
        }

        public void Initialize()
        {
            LeftSideSelectionViewModel = _dataModelUIService.GetDynamicSelectionViewModel(_profileEditorService.SelectedProfileConfiguration.Modules);
            LeftSideSelectionViewModel.PropertySelected += LeftSideSelectionViewModelOnPropertySelected;
            LeftSideSelectionViewModel.LoadEventChildren = false;

            IReadOnlyCollection<DataModelVisualizationRegistration> editors = _dataModelUIService.RegisteredDataModelEditors;
            List<Type> supportedInputTypes = editors.Select(e => e.SupportedType).ToList();
            supportedInputTypes.AddRange(editors.Where(e => e.CompatibleConversionTypes != null).SelectMany(e => e.CompatibleConversionTypes));
            supportedInputTypes.Add(typeof(DataModelEvent));
            supportedInputTypes.Add(typeof(DataModelEvent<>));

            LeftSideSelectionViewModel.FilterTypes = supportedInputTypes.ToArray();
            LeftSideSelectionViewModel.ButtonBrush = new SolidColorBrush(Color.FromRgb(185, 164, 10));
            LeftSideSelectionViewModel.Placeholder = "Select an event";

            Update();
        }

        public override void Update()
        {
            LeftSideSelectionViewModel.ChangeDataModelPath(DataModelConditionEvent.EventPath);

            // Remove VMs of effects no longer applied on the layer
            Items.RemoveRange(Items.Where(c => !DataModelConditionEvent.Children.Contains(c.Model)).ToList());

            if (DataModelConditionEvent.EventPath == null || !DataModelConditionEvent.EventPath.IsValid)
                return;

            TriggerPastParticiple = DataModelConditionEvent.GetDataModelEvent()?.TriggerPastParticiple;
            List<DataModelConditionViewModel> viewModels = new();
            foreach (DataModelConditionPart childModel in Model.Children)
            {
                if (Items.Any(c => c.Model == childModel))
                    continue;
                if (!(childModel is DataModelConditionGroup dataModelConditionGroup))
                    continue;

                DataModelConditionGroupViewModel viewModel = _dataModelConditionsVmFactory.DataModelConditionGroupViewModel(dataModelConditionGroup, ConditionGroupType.Event);
                viewModel.IsRootGroup = true;
                viewModels.Add(viewModel);
            }

            if (viewModels.Any())
                Items.AddRange(viewModels);

            foreach (DataModelConditionViewModel childViewModel in Items)
                childViewModel.Update();
        }

        public override void Evaluate()
        {
            LastTrigger = DataModelConditionEvent.LastTrigger;
            IsConditionMet = DataModelConditionEvent.Evaluate();
        }

        public void ApplyEvent()
        {
            DataModelConditionEvent.UpdateEvent(LeftSideSelectionViewModel.DataModelPath);
            _profileEditorService.SaveSelectedProfileElement();

            Update();
        }

        protected override void OnInitialActivate()
        {
            Initialize();
            base.OnInitialActivate();
        }

        #region Event handlers

        private void LeftSideSelectionViewModelOnPropertySelected(object sender, DataModelInputDynamicEventArgs e)
        {
            ApplyEvent();
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            LeftSideSelectionViewModel.Dispose();
            LeftSideSelectionViewModel.PropertySelected -= LeftSideSelectionViewModelOnPropertySelected;
        }

        #endregion
    }
}
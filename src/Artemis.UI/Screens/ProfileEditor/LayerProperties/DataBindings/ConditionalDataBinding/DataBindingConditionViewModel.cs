using System;
using System.Linq;
using Artemis.Core;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Screens.ProfileEditor.Conditions;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Input;
using Artemis.UI.Shared.Services;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.LayerProperties.DataBindings.ConditionalDataBinding
{
    public sealed class DataBindingConditionViewModel<TLayerProperty, TProperty> : Conductor<DataModelConditionGroupViewModel>, IDisposable
    {
        private readonly IDataModelConditionsVmFactory _dataModelConditionsVmFactory;
        private readonly IDataModelUIService _dataModelUIService;
        private readonly IProfileEditorService _profileEditorService;

        public DataBindingConditionViewModel(DataBindingCondition<TLayerProperty, TProperty> dataBindingCondition,
            IProfileEditorService profileEditorService,
            IDataModelConditionsVmFactory dataModelConditionsVmFactory,
            IDataModelUIService dataModelUIService)
        {
            _profileEditorService = profileEditorService;
            _dataModelConditionsVmFactory = dataModelConditionsVmFactory;
            _dataModelUIService = dataModelUIService;
            DataBindingCondition = dataBindingCondition;
        }

        public DataBindingCondition<TLayerProperty, TProperty> DataBindingCondition { get; }

        public DataModelStaticViewModel ValueViewModel { get; set; }

        protected override void OnInitialActivate()
        {
            base.OnInitialActivate();
            ActiveItem = _dataModelConditionsVmFactory.DataModelConditionGroupViewModel(DataBindingCondition.Condition, ConditionGroupType.General);
            ActiveItem.IsRootGroup = true;
            
            ActiveItem.Update();
            ActiveItem.Updated += ActiveItemOnUpdated;

            ValueViewModel = _dataModelUIService.GetStaticInputViewModel(typeof(TProperty), null);
            ValueViewModel.ValueUpdated += ValueViewModelOnValueUpdated;
            ValueViewModel.Value = DataBindingCondition.Value;
        }

        private void ActiveItemOnUpdated(object sender, EventArgs e)
        {
            if (!ActiveItem.GetChildren().Any())
                DataBindingCondition.ConditionalDataBinding.RemoveCondition(DataBindingCondition);
        }

        private void ValueViewModelOnValueUpdated(object sender, DataModelInputStaticEventArgs e)
        {
            DataBindingCondition.Value = (TProperty) Convert.ChangeType(e.Value, typeof(TProperty));
            _profileEditorService.UpdateSelectedProfileElement();
        }

        public void Evaluate()
        {
            ActiveItem?.Evaluate();
        }

        public void AddCondition()
        {
            ((ConditionalDataBindingModeViewModel<TLayerProperty, TProperty>) Parent).AddCondition();
        }

        public void RemoveCondition()
        {
            ((ConditionalDataBindingModeViewModel<TLayerProperty, TProperty>)Parent).RemoveCondition(DataBindingCondition);

        }

        #region IDisposable

        /// <inheritdoc />
        public void Dispose()
        {
            ValueViewModel?.Dispose();
        }

        #endregion
    }
}
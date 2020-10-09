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
    public class DataBindingConditionViewModel<TLayerProperty, TProperty> : Conductor<DataModelConditionGroupViewModel>, IDisposable
    {
        private readonly IProfileEditorService _profileEditorService;

        public DataBindingConditionViewModel(DataBindingCondition<TLayerProperty, TProperty> dataBindingCondition,
            IProfileEditorService profileEditorService,
            IDataModelConditionsVmFactory dataModelConditionsVmFactory,
            IDataModelUIService dataModelUIService)
        {
            _profileEditorService = profileEditorService;
            DataBindingCondition = dataBindingCondition;

            ActiveItem = dataModelConditionsVmFactory.DataModelConditionGroupViewModel(DataBindingCondition.Condition, false);
            ActiveItem.IsRootGroup = true;
            ActiveItem.Update();
            ActiveItem.Updated += ActiveItemOnUpdated;

            ValueViewModel = dataModelUIService.GetStaticInputViewModel(typeof(TProperty), null);
            ValueViewModel.ValueUpdated += ValueViewModelOnValueUpdated;
            ValueViewModel.Value = DataBindingCondition.Value;
        }

        public DataBindingCondition<TLayerProperty, TProperty> DataBindingCondition { get; }

        public DataModelStaticViewModel ValueViewModel { get; set; }

        public void Dispose()
        {
            ValueViewModel.Dispose();
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
    }
}
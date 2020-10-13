using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Artemis.Core;
using Artemis.UI.Extensions;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Shared.Services;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.LayerProperties.DataBindings.ConditionalDataBinding
{
    public class ConditionalDataBindingModeViewModel<TLayerProperty, TProperty> : Screen, IDataBindingModeViewModel
    {
        private readonly IDataBindingsVmFactory _dataBindingsVmFactory;
        private readonly IProfileEditorService _profileEditorService;
        private bool _updating;

        public ConditionalDataBindingModeViewModel(ConditionalDataBinding<TLayerProperty, TProperty> conditionalDataBinding,
            IProfileEditorService profileEditorService,
            IDataBindingsVmFactory dataBindingsVmFactory)
        {
            _profileEditorService = profileEditorService;
            _dataBindingsVmFactory = dataBindingsVmFactory;

            ConditionalDataBinding = conditionalDataBinding;
            ConditionViewModels = new BindableCollection<DataBindingConditionViewModel<TLayerProperty, TProperty>>();

            Initialize();
        }

        public ConditionalDataBinding<TLayerProperty, TProperty> ConditionalDataBinding { get; }
        public BindableCollection<DataBindingConditionViewModel<TLayerProperty, TProperty>> ConditionViewModels { get; }
        public bool SupportsTestValue => false;

        public void Update()
        {
            UpdateConditionViewModels();
        }

        public object GetTestValue()
        {
            throw new NotSupportedException();
        }

        #region IDisposable

        public void Dispose()
        {
            ConditionalDataBinding.ConditionsUpdated -= ConditionalDataBindingOnConditionsUpdated;

            foreach (DataBindingConditionViewModel<TLayerProperty, TProperty> conditionViewModel in ConditionViewModels)
                conditionViewModel.Dispose();
            ConditionViewModels.Clear();
        }

        #endregion

        private void UpdateConditionViewModels()
        {
            _updating = true;

            // Remove old VMs
            List<DataBindingConditionViewModel<TLayerProperty, TProperty>> toRemove = ConditionViewModels.Where(c => !ConditionalDataBinding.Conditions.Contains(c.DataBindingCondition)).ToList();
            foreach (DataBindingConditionViewModel<TLayerProperty, TProperty> dataBindingConditionViewModel in toRemove)
            {
                ConditionViewModels.Remove(dataBindingConditionViewModel);
                dataBindingConditionViewModel.Dispose();
            }

            // Add missing VMs
            foreach (DataBindingCondition<TLayerProperty, TProperty> condition in ConditionalDataBinding.Conditions)
            {
                if (ConditionViewModels.All(c => c.DataBindingCondition != condition))
                    ConditionViewModels.Add(_dataBindingsVmFactory.DataBindingConditionViewModel(condition));
            }

            // Fix order
            ConditionViewModels.Sort(c => c.DataBindingCondition.Order);

            _updating = false;
        }

        private void Initialize()
        {
            ConditionalDataBinding.ConditionsUpdated += ConditionalDataBindingOnConditionsUpdated;
            ConditionViewModels.CollectionChanged += ConditionViewModelsOnCollectionChanged;
            UpdateConditionViewModels();
        }

        private void ConditionViewModelsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (_updating || e.Action != NotifyCollectionChangedAction.Add)
                return;

            for (int index = 0; index < ConditionViewModels.Count; index++)
            {
                DataBindingConditionViewModel<TLayerProperty, TProperty> conditionViewModel = ConditionViewModels[index];
                conditionViewModel.DataBindingCondition.Order = index + 1;
            }

            ConditionalDataBinding.ApplyOrder();

            _profileEditorService.UpdateSelectedProfileElement();
        }

        private void ConditionalDataBindingOnConditionsUpdated(object sender, EventArgs e)
        {
            UpdateConditionViewModels();
        }
    }
}
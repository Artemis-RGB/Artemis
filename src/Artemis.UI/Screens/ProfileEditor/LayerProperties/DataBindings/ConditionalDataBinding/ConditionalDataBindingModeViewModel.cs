using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Extensions;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Shared.Services;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.LayerProperties.DataBindings.ConditionalDataBinding
{
    public sealed class ConditionalDataBindingModeViewModel<TLayerProperty, TProperty> : Conductor<DataBindingConditionViewModel<TLayerProperty, TProperty>>.Collection.AllActive,
        IDataBindingModeViewModel
    {
        private readonly IDataBindingsVmFactory _dataBindingsVmFactory;
        private readonly ICoreService _coreService;
        private readonly IProfileEditorService _profileEditorService;
        private bool _updating;

        public ConditionalDataBindingModeViewModel(ConditionalDataBinding<TLayerProperty, TProperty> conditionalDataBinding,
            ICoreService coreService,
            IProfileEditorService profileEditorService,
            IDataBindingsVmFactory dataBindingsVmFactory)
        {
            _coreService = coreService;
            _profileEditorService = profileEditorService;
            _dataBindingsVmFactory = dataBindingsVmFactory;

            ConditionalDataBinding = conditionalDataBinding;
        }

        public ConditionalDataBinding<TLayerProperty, TProperty> ConditionalDataBinding { get; }

        public void AddCondition()
        {
            DataBindingCondition<TLayerProperty, TProperty> condition = ConditionalDataBinding.AddCondition();

            // Find the VM of the new condition
            DataBindingConditionViewModel<TLayerProperty, TProperty> viewModel = Items.First(c => c.DataBindingCondition == condition);
            viewModel.ActiveItem.AddCondition();

            _profileEditorService.UpdateSelectedProfileElement();
        }

        protected override void OnInitialActivate()
        {
            Initialize();
            _coreService.FrameRendered += CoreServiceOnFrameRendered;
            base.OnInitialActivate();
        }

        protected override void OnClose()
        {
            _coreService.FrameRendered -= CoreServiceOnFrameRendered;
            base.OnClose();
        }

        private void CoreServiceOnFrameRendered(object sender, FrameRenderedEventArgs e)
        {
            foreach (DataBindingConditionViewModel<TLayerProperty, TProperty> dataBindingConditionViewModel in Items)
                dataBindingConditionViewModel.Evaluate();
        }

        private void UpdateItems()
        {
            _updating = true;

            // Remove old VMs
            List<DataBindingConditionViewModel<TLayerProperty, TProperty>> toRemove = Items.Where(c => !ConditionalDataBinding.Conditions.Contains(c.DataBindingCondition)).ToList();
            foreach (DataBindingConditionViewModel<TLayerProperty, TProperty> dataBindingConditionViewModel in toRemove)
            {
                Items.Remove(dataBindingConditionViewModel);
                dataBindingConditionViewModel.Dispose();
            }

            // Add missing VMs
            foreach (DataBindingCondition<TLayerProperty, TProperty> condition in ConditionalDataBinding.Conditions)
                if (Items.All(c => c.DataBindingCondition != condition))
                    Items.Add(_dataBindingsVmFactory.DataBindingConditionViewModel(condition));

            // Fix order
            ((BindableCollection<DataBindingConditionViewModel<TLayerProperty, TProperty>>) Items).Sort(c => c.DataBindingCondition.Order);

            _updating = false;
        }

        private void Initialize()
        {
            ConditionalDataBinding.ConditionsUpdated += ConditionalDataBindingOnConditionsUpdated;
            Items.CollectionChanged += ItemsOnCollectionChanged;
            UpdateItems();
        }

        private void ItemsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (_updating || e.Action != NotifyCollectionChangedAction.Add)
                return;

            for (int index = 0; index < Items.Count; index++)
            {
                DataBindingConditionViewModel<TLayerProperty, TProperty> conditionViewModel = Items[index];
                conditionViewModel.DataBindingCondition.Order = index + 1;
            }

            ConditionalDataBinding.ApplyOrder();

            _profileEditorService.UpdateSelectedProfileElement();
        }

        private void ConditionalDataBindingOnConditionsUpdated(object sender, EventArgs e)
        {
            UpdateItems();
        }

        public bool SupportsTestValue => false;

        public void Update()
        {
            UpdateItems();
        }

        public object GetTestValue()
        {
            throw new NotSupportedException();
        }

        #region IDisposable

        /// <inheritdoc />
        public void Dispose()
        {
            ConditionalDataBinding.ConditionsUpdated -= ConditionalDataBindingOnConditionsUpdated;

            foreach (DataBindingConditionViewModel<TLayerProperty, TProperty> conditionViewModel in Items)
                conditionViewModel.Dispose();
            Items.Clear();
        }

        #endregion
    }
}
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Artemis.Core;
using Artemis.UI.Extensions;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Input;
using Artemis.UI.Shared.Services;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.LayerProperties.DataBindings.DirectDataBinding
{
    public class DirectDataBindingModeViewModel<TLayerProperty, TProperty> : Screen, IDataBindingModeViewModel
    {
        private readonly IDataBindingsVmFactory _dataBindingsVmFactory;
        private readonly IDataModelUIService _dataModelUIService;
        private readonly IProfileEditorService _profileEditorService;
        private bool _canAddModifier;
        private DataModelDynamicViewModel _targetSelectionViewModel;
        private bool _updating;

        public DirectDataBindingModeViewModel(DirectDataBinding<TLayerProperty, TProperty> directDataBinding,
            IProfileEditorService profileEditorService,
            IDataModelUIService dataModelUIService,
            IDataBindingsVmFactory dataBindingsVmFactory)
        {
            _profileEditorService = profileEditorService;
            _dataModelUIService = dataModelUIService;
            _dataBindingsVmFactory = dataBindingsVmFactory;

            DirectDataBinding = directDataBinding;
            ModifierViewModels = new BindableCollection<DataBindingModifierViewModel<TLayerProperty, TProperty>>();

            Initialize();
        }


        public DirectDataBinding<TLayerProperty, TProperty> DirectDataBinding { get; }
        public BindableCollection<DataBindingModifierViewModel<TLayerProperty, TProperty>> ModifierViewModels { get; }

        public bool CanAddModifier
        {
            get => _canAddModifier;
            set => SetAndNotify(ref _canAddModifier, value);
        }

        public DataModelDynamicViewModel TargetSelectionViewModel
        {
            get => _targetSelectionViewModel;
            private set => SetAndNotify(ref _targetSelectionViewModel, value);
        }

        public bool SupportsTestValue => true;

        public void Update()
        {
            TargetSelectionViewModel.PopulateSelectedPropertyViewModel(DirectDataBinding.SourceDataModel, DirectDataBinding.SourcePropertyPath);
            TargetSelectionViewModel.FilterTypes = new[] {DirectDataBinding.DataBinding.GetTargetType()};

            CanAddModifier = DirectDataBinding.SourceDataModel != null;
            UpdateModifierViewModels();
        }

        public object GetTestValue()
        {
            return TargetSelectionViewModel.SelectedPropertyViewModel?.GetCurrentValue();
        }

        #region IDisposable

        public void Dispose()
        {
            TargetSelectionViewModel.PropertySelected -= TargetSelectionViewModelOnPropertySelected;
            TargetSelectionViewModel.Dispose();
            DirectDataBinding.ModifiersUpdated -= DirectDataBindingOnModifiersUpdated;

            foreach (DataBindingModifierViewModel<TLayerProperty, TProperty> dataBindingModifierViewModel in ModifierViewModels)
                dataBindingModifierViewModel.Dispose();
            ModifierViewModels.Clear();
        }

        #endregion

        private void Initialize()
        {
            DirectDataBinding.ModifiersUpdated += DirectDataBindingOnModifiersUpdated;
            TargetSelectionViewModel = _dataModelUIService.GetDynamicSelectionViewModel(_profileEditorService.GetCurrentModule());
            TargetSelectionViewModel.PropertySelected += TargetSelectionViewModelOnPropertySelected;
            ModifierViewModels.CollectionChanged += ModifierViewModelsOnCollectionChanged;
            Update();
        }

        private void ModifierViewModelsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (_updating || e.Action != NotifyCollectionChangedAction.Add)
                return;

            for (int index = 0; index < ModifierViewModels.Count; index++)
            {
                DataBindingModifierViewModel<TLayerProperty, TProperty> dataBindingModifierViewModel = ModifierViewModels[index];
                dataBindingModifierViewModel.Modifier.Order = index + 1;
            }

            DirectDataBinding.ApplyOrder();

            _profileEditorService.UpdateSelectedProfileElement();
        }

        #region Target

        private void TargetSelectionViewModelOnPropertySelected(object sender, DataModelInputDynamicEventArgs e)
        {
            DirectDataBinding.UpdateSource(e.DataModelVisualizationViewModel.DataModel, e.DataModelVisualizationViewModel.Path);
            Update();

            _profileEditorService.UpdateSelectedProfileElement();
        }

        #endregion

        #region Modifiers

        public void AddModifier()
        {
            DirectDataBinding.AddModifier(ProfileRightSideType.Dynamic);
            _profileEditorService.UpdateSelectedProfileElement();
        }

        private void UpdateModifierViewModels()
        {
            _updating = true;

            // Remove old VMs
            List<DataBindingModifierViewModel<TLayerProperty, TProperty>> toRemove = ModifierViewModels.Where(m => !DirectDataBinding.Modifiers.Contains(m.Modifier)).ToList();
            foreach (DataBindingModifierViewModel<TLayerProperty, TProperty> modifierViewModel in toRemove)
            {
                ModifierViewModels.Remove(modifierViewModel);
                modifierViewModel.Dispose();
            }

            // Add missing VMs
            foreach (DataBindingModifier<TLayerProperty, TProperty> modifier in DirectDataBinding.Modifiers)
            {
                if (ModifierViewModels.All(m => m.Modifier != modifier))
                    ModifierViewModels.Add(_dataBindingsVmFactory.DataBindingModifierViewModel(modifier));
            }

            // Fix order
            ModifierViewModels.Sort(m => m.Modifier.Order);

            _updating = false;
        }

        private void DirectDataBindingOnModifiersUpdated(object sender, EventArgs e)
        {
            UpdateModifierViewModels();
        }

        #endregion
    }
}
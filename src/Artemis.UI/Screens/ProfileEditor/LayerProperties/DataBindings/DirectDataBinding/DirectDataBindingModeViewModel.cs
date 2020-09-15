using System;
using Artemis.Core;
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
        private DataModelDynamicViewModel _targetSelectionViewModel;

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

        public DataModelDynamicViewModel TargetSelectionViewModel
        {
            get => _targetSelectionViewModel;
            private set => SetAndNotify(ref _targetSelectionViewModel, value);
        }

        public void Update()
        {
            TargetSelectionViewModel.PopulateSelectedPropertyViewModel(DirectDataBinding.SourceDataModel, DirectDataBinding.SourcePropertyPath);
            TargetSelectionViewModel.FilterTypes = new[] {DirectDataBinding.DataBinding.GetTargetType()};

            UpdateModifierViewModels();
        }

        public object GetTestValue()
        {
            return TargetSelectionViewModel.SelectedPropertyViewModel?.GetCurrentValue();
        }
        
        private void Initialize()
        {
            DirectDataBinding.ModifiersUpdated += DirectDataBindingOnModifiersUpdated;
            TargetSelectionViewModel = _dataModelUIService.GetDynamicSelectionViewModel(_profileEditorService.GetCurrentModule());
            TargetSelectionViewModel.PropertySelected += TargetSelectionViewModelOnPropertySelected;

            Update();
        }
        
        #region Target

        private void TargetSelectionViewModelOnPropertySelected(object sender, DataModelInputDynamicEventArgs e)
        {
            DirectDataBinding.UpdateSource(e.DataModelVisualizationViewModel.DataModel, e.DataModelVisualizationViewModel.PropertyPath);
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
            foreach (var dataBindingModifierViewModel in ModifierViewModels)
                dataBindingModifierViewModel.Dispose();
            ModifierViewModels.Clear();

            foreach (var dataBindingModifier in DirectDataBinding.Modifiers)
                ModifierViewModels.Add(_dataBindingsVmFactory.DataBindingModifierViewModel(dataBindingModifier));
        }

        private void DirectDataBindingOnModifiersUpdated(object sender, EventArgs e)
        {
            UpdateModifierViewModels();
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            TargetSelectionViewModel.PropertySelected -= TargetSelectionViewModelOnPropertySelected;
            TargetSelectionViewModel.Dispose();
            DirectDataBinding.ModifiersUpdated -= DirectDataBindingOnModifiersUpdated;

            foreach (var dataBindingModifierViewModel in ModifierViewModels)
                dataBindingModifierViewModel.Dispose();
            ModifierViewModels.Clear();
        }

        #endregion
    }
}
using System;
using System.Linq;
using Artemis.Core;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.LayerProperties.Tree
{
    public sealed class TreePropertyViewModel<T> : Screen, ITreePropertyViewModel
    {
        private readonly IProfileEditorService _profileEditorService;
        private PropertyInputViewModel<T> _propertyInputViewModel;

        public TreePropertyViewModel(LayerProperty<T> layerProperty, LayerPropertyViewModel layerPropertyViewModel, IProfileEditorService profileEditorService)
        {
            _profileEditorService = profileEditorService;
            LayerProperty = layerProperty;
            LayerPropertyViewModel = layerPropertyViewModel;

            PropertyInputViewModel = _profileEditorService.CreatePropertyInputViewModel(LayerProperty);
            PropertyInputViewModel.ConductWith(this);
        }

        public LayerProperty<T> LayerProperty { get; }
        public LayerPropertyViewModel LayerPropertyViewModel { get; }

        public PropertyInputViewModel<T> PropertyInputViewModel
        {
            get => _propertyInputViewModel;
            set => SetAndNotify(ref _propertyInputViewModel, value);
        }

        public bool KeyframesEnabled
        {
            get => LayerProperty.KeyframesEnabled;
            set => ApplyKeyframesEnabled(value);
        }

        public void ActivateDataBindingViewModel()
        {
            if (_profileEditorService.SelectedDataBinding == LayerProperty)
                _profileEditorService.ChangeSelectedDataBinding(null);
            else
                _profileEditorService.ChangeSelectedDataBinding(LayerProperty);
        }

        public void ResetToDefault()
        {
            LayerProperty.ApplyDefaultValue(_profileEditorService.CurrentTime);
            _profileEditorService.UpdateSelectedProfileElement();
        }

        private void ApplyKeyframesEnabled(bool enable)
        {
            // If enabling keyframes for the first time, add a keyframe with the current value at the current position
            if (enable && !LayerProperty.Keyframes.Any())
                LayerProperty.AddKeyframe(new LayerPropertyKeyframe<T>(
                    LayerProperty.CurrentValue,
                    _profileEditorService.CurrentTime,
                    Easings.Functions.Linear,
                    LayerProperty
                ));
            // If disabling keyframes, set the base value to the current value
            else if (!enable && LayerProperty.Keyframes.Any())
                LayerProperty.BaseValue = LayerProperty.CurrentValue;

            LayerProperty.KeyframesEnabled = enable;

            _profileEditorService.UpdateSelectedProfileElement();
        }

        public bool HasDataBinding => LayerProperty.HasDataBinding;

        public double GetDepth()
        {
            int depth = 0;
            LayerPropertyGroup current = LayerProperty.LayerPropertyGroup;
            while (current != null)
            {
                depth++;
                current = current.Parent;
            }

            return depth;
        }
        
        #region Overrides of Screen

        /// <inheritdoc />
        protected override void OnInitialActivate()
        {
            _profileEditorService.SelectedDataBindingChanged += ProfileEditorServiceOnSelectedDataBindingChanged;
            LayerProperty.VisibilityChanged += LayerPropertyOnVisibilityChanged;
            LayerProperty.DataBindingEnabled += LayerPropertyOnDataBindingChange;
            LayerProperty.DataBindingDisabled += LayerPropertyOnDataBindingChange;
            LayerProperty.KeyframesToggled += LayerPropertyOnKeyframesToggled;
            LayerPropertyViewModel.IsVisible = !LayerProperty.IsHidden;
            base.OnInitialActivate();
        }

        /// <inheritdoc />
        protected override void OnClose()
        {
            _profileEditorService.SelectedDataBindingChanged -= ProfileEditorServiceOnSelectedDataBindingChanged;
            LayerProperty.VisibilityChanged -= LayerPropertyOnVisibilityChanged;
            LayerProperty.DataBindingEnabled -= LayerPropertyOnDataBindingChange;
            LayerProperty.DataBindingDisabled -= LayerPropertyOnDataBindingChange;
            LayerProperty.KeyframesToggled -= LayerPropertyOnKeyframesToggled;
            base.OnClose();
        }

        #endregion

        #region Event handlers

        private void ProfileEditorServiceOnSelectedDataBindingChanged(object sender, EventArgs e)
        {
            LayerPropertyViewModel.IsHighlighted = _profileEditorService.SelectedDataBinding == LayerProperty;
        }

        private void LayerPropertyOnVisibilityChanged(object sender, LayerPropertyEventArgs e)
        {
            LayerPropertyViewModel.IsVisible = !LayerProperty.IsHidden;
        }

        private void LayerPropertyOnDataBindingChange(object sender, LayerPropertyEventArgs e)
        {
            NotifyOfPropertyChange(nameof(HasDataBinding));
        }

        private void LayerPropertyOnKeyframesToggled(object sender, LayerPropertyEventArgs e)
        {
            NotifyOfPropertyChange(nameof(KeyframesEnabled));
        }

        #endregion
    }

    public interface ITreePropertyViewModel : IScreen
    {
        bool HasDataBinding { get; }
        double GetDepth();
    }
}
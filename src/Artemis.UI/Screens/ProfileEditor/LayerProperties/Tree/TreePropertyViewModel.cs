using System;
using System.ComponentModel;
using System.Linq;
using Artemis.Core;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.LayerProperties.Tree
{
    public class TreePropertyViewModel<T> : Screen, ITreePropertyViewModel
    {
        private readonly IProfileEditorService _profileEditorService;
        private PropertyInputViewModel<T> _propertyInputViewModel;

        public TreePropertyViewModel(LayerProperty<T> layerProperty, LayerPropertyViewModel layerPropertyViewModel, IProfileEditorService profileEditorService)
        {
            _profileEditorService = profileEditorService;
            LayerProperty = layerProperty;
            LayerPropertyViewModel = layerPropertyViewModel;

            PropertyInputViewModel = _profileEditorService.CreatePropertyInputViewModel(LayerProperty);
            _profileEditorService.SelectedDataBindingChanged += ProfileEditorServiceOnSelectedDataBindingChanged;
            LayerProperty.VisibilityChanged += LayerPropertyOnVisibilityChanged;

            LayerPropertyViewModel.IsVisible = !LayerProperty.IsHidden;
        }

        public LayerProperty<T> LayerProperty { get; }
        public LayerPropertyViewModel LayerPropertyViewModel { get; }

        public PropertyInputViewModel<T> PropertyInputViewModel
        {
            get => _propertyInputViewModel;
            set => SetAndNotify(ref _propertyInputViewModel, value);
        }

        public bool HasPropertyInputViewModel => PropertyInputViewModel != null;

        public bool KeyframesEnabled
        {
            get => LayerProperty.KeyframesEnabled;
            set => ApplyKeyframesEnabled(value);
        }

        public bool DataBindingsOpen
        {
            get => _profileEditorService.SelectedDataBinding == LayerProperty;
            set => _profileEditorService.ChangeSelectedDataBinding(value ? LayerProperty : null);
        }

        #region IDisposable

        public void Dispose()
        {
            _propertyInputViewModel?.Dispose();
            _profileEditorService.SelectedDataBindingChanged -= ProfileEditorServiceOnSelectedDataBindingChanged;
            LayerProperty.VisibilityChanged -= LayerPropertyOnVisibilityChanged;
        }

        #endregion

        private void ApplyKeyframesEnabled(bool enable)
        {
            // If enabling keyframes for the first time, add a keyframe with the current value at the current position
            if (enable && !LayerProperty.Keyframes.Any())
            {
                LayerProperty.AddKeyframe(new LayerPropertyKeyframe<T>(
                    LayerProperty.CurrentValue,
                    _profileEditorService.CurrentTime,
                    Easings.Functions.Linear,
                    LayerProperty
                ));
            }
            // If disabling keyframes, set the base value to the current value
            else if (!enable && LayerProperty.Keyframes.Any())
                LayerProperty.BaseValue = LayerProperty.CurrentValue;

            LayerProperty.KeyframesEnabled = enable;

            _profileEditorService.UpdateSelectedProfileElement();
        }

        #region Event handlers

        private void ProfileEditorServiceOnSelectedDataBindingChanged(object sender, EventArgs e)
        {
            NotifyOfPropertyChange(nameof(DataBindingsOpen));
        }

        private void LayerPropertyOnVisibilityChanged(object? sender, LayerPropertyEventArgs<T> e)
        {
            LayerPropertyViewModel.IsVisible = !LayerProperty.IsHidden;
        }

        #endregion
    }

    public interface ITreePropertyViewModel : IScreen, INotifyPropertyChanged, IDisposable
    {
        bool HasPropertyInputViewModel { get; }
        bool DataBindingsOpen { get; set; }
    }
}
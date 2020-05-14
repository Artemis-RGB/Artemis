using System;
using Stylet;

namespace Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.Tree.PropertyInput
{
    public abstract class PropertyInputViewModel<T> : PropertyChangedBase, IDisposable
    {
        protected PropertyInputViewModel(LayerPropertyViewModel<T> layerPropertyViewModel)
        {
            LayerPropertyViewModel = layerPropertyViewModel;
            LayerPropertyViewModel.LayerProperty.Updated += LayerPropertyOnUpdated;
        }

        public LayerPropertyViewModel<T> LayerPropertyViewModel { get; }

        public bool InputDragging { get; private set; }

        protected T InputValue
        {
            get => LayerPropertyViewModel.LayerProperty.CurrentValue;
            set => LayerPropertyViewModel.SetCurrentValue(value, !InputDragging);
        }

        public abstract void Update();

        private void LayerPropertyOnUpdated(object? sender, EventArgs e)
        {
            Update();
        }

        #region Event handlers

        public void InputDragStarted(object sender, EventArgs e)
        {
            InputDragging = true;
        }

        public void InputDragEnded(object sender, EventArgs e)
        {
            InputDragging = false;
            LayerPropertyViewModel.ProfileEditorService.UpdateSelectedProfileElement();
        }

        #endregion

        public void Dispose()
        {
            LayerPropertyViewModel.LayerProperty.Updated -= LayerPropertyOnUpdated;
        }
    }
}
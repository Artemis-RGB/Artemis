using System;
using System.Collections.Generic;
using Artemis.UI.Exceptions;
using Artemis.UI.Services.Interfaces;
using Stylet;

namespace Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.PropertyTree.PropertyInput
{
    public abstract class PropertyInputViewModel : PropertyChangedBase
    {
        protected PropertyInputViewModel(IProfileEditorService profileEditorService)
        {
            ProfileEditorService = profileEditorService;
        }

        protected IProfileEditorService ProfileEditorService { get; }
        public abstract List<Type> CompatibleTypes { get; }

        public bool Initialized { get; private set; }
        public bool InputDragging { get; private set; }
        public LayerPropertyViewModel LayerPropertyViewModel { get; private set; }

        protected object InputValue
        {
            get => LayerPropertyViewModel.LayerProperty.GetCurrentValue();
            set => UpdateInputValue(value);
        }

        public void Initialize(LayerPropertyViewModel layerPropertyViewModel)
        {
            var type = layerPropertyViewModel.LayerProperty.Type;
            if (type.IsEnum)
                type = typeof(Enum);
            if (Initialized)
                throw new ArtemisUIException("Cannot initialize the same property input VM twice");
            if (!CompatibleTypes.Contains(type))
                throw new ArtemisUIException($"This input VM does not support the provided type {type.Name}");

            LayerPropertyViewModel = layerPropertyViewModel;
            layerPropertyViewModel.LayerProperty.ValueChanged += (sender, args) => Update();
            Update();

            Initialized = true;

            OnInitialized();
        }

        public abstract void Update();

        protected virtual void OnInitialized()
        {
        }

        private void UpdateInputValue(object value)
        {
            LayerPropertyViewModel.LayerProperty.SetCurrentValue(value, ProfileEditorService.CurrentTime);
            // Force the keyframe engine to update, the edited keyframe might affect the current keyframe progress
            LayerPropertyViewModel.LayerProperty.KeyframeEngine?.Update(0);

            if (!InputDragging)
                ProfileEditorService.UpdateSelectedProfileElement();
            else
                ProfileEditorService.UpdateProfilePreview();
        }

        #region Event handlers

        public void InputDragStarted(object sender, EventArgs e)
        {
            InputDragging = true;
        }

        public void InputDragEnded(object sender, EventArgs e)
        {
            InputDragging = false;
            ProfileEditorService.UpdateSelectedProfileElement();
        }

        #endregion
    }
}
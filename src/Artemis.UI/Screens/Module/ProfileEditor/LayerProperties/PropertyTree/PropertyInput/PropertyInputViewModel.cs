using System;
using System.Collections.Generic;
using System.Linq;
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

        protected IProfileEditorService ProfileEditorService { get; set; }

        public bool Initialized { get; private set; }

        public abstract List<Type> CompatibleTypes { get; }
        public LayerPropertyViewModel LayerPropertyViewModel { get; private set; }

        protected object InputValue
        {
            get => LayerPropertyViewModel.LayerProperty.GetCurrentValue();
            set => UpdateInputValue(value);
        }

        public void Initialize(LayerPropertyViewModel layerPropertyViewModel)
        {
            if (Initialized)
                throw new ArtemisUIException("Cannot initialize the same property input VM twice");
            if (!CompatibleTypes.Contains(layerPropertyViewModel.LayerProperty.Type))
                throw new ArtemisUIException($"This input VM does not support the provided type {layerPropertyViewModel.LayerProperty.Type.Name}");

            LayerPropertyViewModel = layerPropertyViewModel;
            Update();

            Initialized = true;
        }

        private void UpdateInputValue(object value)
        {
            LayerPropertyViewModel.LayerProperty.SetCurrentValue(value, ProfileEditorService.CurrentTime);
            // Force the keyframe engine to update, the edited keyframe might affect the current keyframe progress
            LayerPropertyViewModel.LayerProperty.KeyframeEngine.Update(0);

            ProfileEditorService.UpdateSelectedProfileElement();
        }

        public abstract void Update();
    }
}
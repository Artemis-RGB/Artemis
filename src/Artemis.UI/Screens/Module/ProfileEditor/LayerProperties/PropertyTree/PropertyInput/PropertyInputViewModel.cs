using System;
using System.Collections.Generic;
using Artemis.UI.Exceptions;
using Stylet;

namespace Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.PropertyTree.PropertyInput
{
    public abstract class PropertyInputViewModel : PropertyChangedBase
    {
        public bool Initialized { get; private set; }

        public abstract List<Type> CompatibleTypes { get; }
        public LayerPropertyViewModel LayerPropertyViewModel { get; private set; }

        public void Initialize(LayerPropertyViewModel layerPropertyViewModel)
        {
            if (Initialized)
                throw new ArtemisUIException("Cannot initialize the same property input VM twice");
            if (!CompatibleTypes.Contains(layerPropertyViewModel.LayerProperty.Type))
                throw new ArtemisUIException($"This input VM does not support the provided type {layerPropertyViewModel.LayerProperty.Type.Name}");

            LayerPropertyViewModel = layerPropertyViewModel;
            Initialized = true;
        }
    }
}
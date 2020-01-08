using System.Collections.Generic;
using System.Linq;
using Artemis.Core.Models.Profile.LayerProperties;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.PropertyTree.PropertyInput;
using Ninject;
using Stylet;

namespace Artemis.UI.Screens.Module.ProfileEditor.LayerProperties
{
    public class LayerPropertyViewModel : PropertyChangedBase
    {
        private readonly IKernel _kernel;
        private bool _keyframesEnabled;

        public LayerPropertyViewModel(BaseLayerProperty layerProperty, LayerPropertyViewModel parent, ILayerPropertyViewModelFactory layerPropertyViewModelFactory, IKernel kernel)
        {
            _kernel = kernel;

            LayerProperty = layerProperty;
            Parent = parent;
            Children = new List<LayerPropertyViewModel>();

            foreach (var child in layerProperty.Children)
                Children.Add(layerPropertyViewModelFactory.Create(child, this));
        }

        public BaseLayerProperty LayerProperty { get; }

        public LayerPropertyViewModel Parent { get; }
        public List<LayerPropertyViewModel> Children { get; set; }

        public bool IsExpanded { get; set; }

        public bool KeyframesEnabled
        {
            get => _keyframesEnabled;
            set
            {
                _keyframesEnabled = value;
                UpdateKeyframes();
            }
        }

        private void UpdateKeyframes()
        {
        }

        public PropertyInputViewModel GetPropertyInputViewModel()
        {
            var match = _kernel.Get<List<PropertyInputViewModel>>().FirstOrDefault(p => p.CompatibleTypes.Contains(LayerProperty.Type));
            if (match == null)
                return null;

            match.Initialize(this);
            return match;
        }
    }
}
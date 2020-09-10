using System;
using Artemis.Core;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Screens.ProfileEditor.LayerProperties.Tree;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.LayerProperties
{
    public class LayerPropertyGroupViewModel : PropertyChangedBase, IDisposable
    {
        private readonly ILayerPropertyVmFactory _layerPropertyVmFactory;

        public LayerPropertyGroupViewModel(LayerPropertyGroup layerPropertyGroup, ILayerPropertyVmFactory layerPropertyVmFactory)
        {
            _layerPropertyVmFactory = layerPropertyVmFactory;

            LayerPropertyGroup = layerPropertyGroup;
            LayerPropertyGroupTreeViewModel = layerPropertyVmFactory.LayerPropertyGroupTreeViewModel(this);
            PopulateChildren();
        }

        public LayerPropertyGroup LayerPropertyGroup { get; }
        public LayerPropertyGroupTreeViewModel LayerPropertyGroupTreeViewModel { get; }
        public BindableCollection<PropertyChangedBase> Children { get; set; }

        public bool IsVisible => !LayerPropertyGroup.IsHidden;

        public bool IsExpanded
        {
            get => LayerPropertyGroup.ProfileElement.IsPropertyGroupExpanded(LayerPropertyGroup);
            set
            {
                LayerPropertyGroup.ProfileElement.SetPropertyGroupExpanded(LayerPropertyGroup, value);
                NotifyOfPropertyChange(nameof(IsExpanded));
            }
        }

        private void PopulateChildren()
        {
            // Get all properties and property groups and create VMs for them
            // The group has methods for getting this without reflection but then we lose the order of the properties as they are defined on the group
            foreach (var propertyInfo in LayerPropertyGroup.GetType().GetProperties())
            {
                var propertyAttribute = (PropertyDescriptionAttribute) Attribute.GetCustomAttribute(propertyInfo, typeof(PropertyDescriptionAttribute));
                var groupAttribute = (PropertyGroupDescriptionAttribute) Attribute.GetCustomAttribute(propertyInfo, typeof(PropertyGroupDescriptionAttribute));
                var value = propertyInfo.GetValue(LayerPropertyGroup);

                // Create VMs for properties on the group
                if (propertyAttribute != null && value is ILayerProperty layerProperty)
                {
                    var layerPropertyViewModel = _layerPropertyVmFactory.LayerPropertyViewModel(layerProperty);
                    // After creation ensure a supported input VM was found, if not, discard the VM
                    if (!layerPropertyViewModel.LayerPropertyTreeViewModel.HasPropertyInputViewModel)
                        layerPropertyViewModel.Dispose();
                    else
                        Children.Add(layerPropertyViewModel);
                }
                // Create VMs for child groups on this group, resulting in a nested structure
                else if (groupAttribute != null && value is LayerPropertyGroup layerPropertyGroup)
                    Children.Add(_layerPropertyVmFactory.LayerPropertyGroupViewModel(layerPropertyGroup));
            }
        }

        public void Dispose()
        {
            foreach (var child in Children)
            {
                if (child is IDisposable disposableChild)
                    disposableChild.Dispose();
            }
        }
    }
}
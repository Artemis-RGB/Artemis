using System;
using System.Reflection;

namespace Artemis.Core
{
    public class DataBindingRegistration
    {
        internal DataBindingRegistration(BaseLayerProperty layerProperty, PropertyInfo property, IDataBindingConverter converter)
        {
            LayerProperty = layerProperty ?? throw new ArgumentNullException(nameof(layerProperty));
            Property = property ?? throw new ArgumentNullException(nameof(property));
            Converter = converter ?? throw new ArgumentNullException(nameof(converter));
        }

        public BaseLayerProperty LayerProperty { get; set; }
        public PropertyInfo Property { get; set; }
        public IDataBindingConverter Converter { get; set; }
    }
}
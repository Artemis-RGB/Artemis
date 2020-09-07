using System;
using System.Reflection;

namespace Artemis.Core
{
    public class DataBindingRegistration
    {
        internal DataBindingRegistration(BaseLayerProperty layerProperty, PropertyInfo property, DataBindingConverter converter, string path)
        {
            LayerProperty = layerProperty ?? throw new ArgumentNullException(nameof(layerProperty));
            Property = property ?? throw new ArgumentNullException(nameof(property));
            Converter = converter ?? throw new ArgumentNullException(nameof(converter));
            Path = path;
        }

        public DataBinding DataBinding { get; internal set; }
        public BaseLayerProperty LayerProperty { get; }
        public PropertyInfo Property { get; }
        public DataBindingConverter Converter { get; }
        public string Path { get; }
    }
}
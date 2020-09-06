using System;

namespace Artemis.Core
{
    /// <inheritdoc />
    public class IntDataBindingConverter : IDataBindingConverter
    {
        /// <inheritdoc />
        public Type SupportedType => typeof(int);

        /// <inheritdoc />
        public bool SupportsSum => true;

        /// <inheritdoc />
        public bool SupportsInterpolate => true;

        /// <inheritdoc />
        public object Sum(BaseLayerProperty layerProperty, object a, object b)
        {
            return (int) a + (int) b;
        }

        /// <inheritdoc />
        public object Interpolate(BaseLayerProperty layerProperty, object a, object b, float progress)
        {
            var diff = (int) b - (int) a;
            return diff * progress;
        }

        /// <inheritdoc />
        public void ApplyValue(BaseLayerProperty layerProperty, object value)
        {
            layerProperty.CurrentValue = value;
        }

        /// <inheritdoc />
        public object GetValue(BaseLayerProperty layerProperty)
        {
            return layerProperty.CurrentValue;
        }
    }
}
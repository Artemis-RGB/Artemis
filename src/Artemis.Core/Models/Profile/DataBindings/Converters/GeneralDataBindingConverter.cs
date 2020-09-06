using System;

namespace Artemis.Core
{
    /// <inheritdoc />
    public class GeneralDataBindingConverter : IDataBindingConverter
    {
        /// <inheritdoc />
        public Type SupportedType => typeof(object);

        /// <inheritdoc />
        public bool SupportsSum => false;

        /// <inheritdoc />
        public bool SupportsInterpolate => false;

        /// <inheritdoc />
        public object Sum(BaseLayerProperty layerProperty, object a, object b)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public object Interpolate(BaseLayerProperty layerProperty, object a, object b, float progress)
        {
            throw new NotSupportedException();
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
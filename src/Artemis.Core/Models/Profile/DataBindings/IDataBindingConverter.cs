using System;

namespace Artemis.Core
{
    /// <summary>
    ///     A data binding converter that acts as the bridge between a <see cref="DataBinding" /> and a
    ///     <see cref="LayerProperty{T}" />
    /// </summary>
    public interface IDataBindingConverter
    {
        /// <summary>
        ///     Gets the type this converter supports
        /// </summary>
        Type SupportedType { get; }

        /// <summary>
        ///     Gets whether or not this data binding converter supports the <see cref="Sum" /> method
        /// </summary>
        bool SupportsSum { get; }

        /// <summary>
        ///     Gets whether or not this data binding converter supports the <see cref="Interpolate" /> method
        /// </summary>
        bool SupportsInterpolate { get; }

        /// <summary>
        ///     Returns the sum of <paramref name="a" /> and <paramref name="b" />
        /// </summary>
        /// <param name="layerProperty">The layer property this sum is done for</param>
        object Sum(BaseLayerProperty layerProperty, object a, object b);

        /// <summary>
        ///     Returns the the interpolated value between <paramref name="a" /> and <paramref name="b" /> on a scale (generally)
        ///     between <c>0.0</c> and <c>1.0</c> defined by the <paramref name="progress" />
        ///     <para>Note: The progress may go be negative or go beyond <c>1.0</c> depending on the easing method used</para>
        /// </summary>
        /// <param name="layerProperty">The layer property this interpolation is done for</param>
        /// <param name="a">The value to interpolate away from</param>
        /// <param name="b">The value to interpolate towards</param>
        /// <param name="progress">The progress of the interpolation between 0.0 and 1.0</param>
        /// <returns></returns>
        object Interpolate(BaseLayerProperty layerProperty, object a, object b, float progress);

        /// <summary>
        ///     Applies the <paramref name="value" /> to the layer property
        /// </summary>
        /// <param name="layerProperty">The layer property this value is to be applied to</param>
        /// <param name="value"></param>
        void ApplyValue(BaseLayerProperty layerProperty, object value);

        /// <summary>
        ///     Returns the current base value of the data binding
        /// </summary>
        /// <param name="layerProperty">The layer property this value must be retrieved from</param>
        object GetValue(BaseLayerProperty layerProperty);
    }
}
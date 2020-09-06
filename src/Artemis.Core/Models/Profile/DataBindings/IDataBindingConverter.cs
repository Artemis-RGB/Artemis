namespace Artemis.Core
{
    /// <summary>
    ///     A data binding converter that acts as the bridge between a <see cref="DataBinding" /> and a
    ///     <see cref="LayerProperty{T}" />
    /// </summary>
    public interface IDataBindingConverter
    {
        public BaseLayerProperty BaseLayerProperty { get; set; }

        /// <summary>
        ///     Returns the sum of <paramref name="a" /> and <paramref name="b" />
        /// </summary>
        object Sum(object a, object b);

        /// <summary>
        ///     Returns the the interpolated value between <paramref name="a" /> and <paramref name="b" /> on a scale (generally)
        ///     between <c>0.0</c> and <c>1.0</c> defined by the <paramref name="progress" />
        ///     <para>Note: The progress may go be negative or go beyond <c>1.0</c> depending on the easing method used</para>
        /// </summary>
        /// <param name="a">The value to interpolate away from</param>
        /// <param name="b">The value to interpolate towards</param>
        /// <param name="progress">The progress of the interpolation between 0.0 and 1.0</param>
        /// <returns></returns>
        object Interpolate(object a, object b, float progress);

        /// <summary>
        ///     Applies the <paramref name="value" /> to the layer property
        /// </summary>
        /// <param name="value"></param>
        void ApplyValue(object value);

        /// <summary>
        ///     Returns the current base value of the data binding
        /// </summary>
        object GetValue();
    }
}
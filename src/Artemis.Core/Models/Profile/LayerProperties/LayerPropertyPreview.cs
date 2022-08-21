using System;
using System.Linq;

namespace Artemis.Core;

/// <summary>
///     Represents a container for a preview value of a <see cref="LayerProperty{T}" /> that can be used to update and
///     discard a temporary value.
/// </summary>
/// <typeparam name="T">The value type of the layer property.</typeparam>
public sealed class LayerPropertyPreview<T> : IDisposable
{
    /// <summary>
    ///     Creates a new instance of the <see cref="LayerPropertyPreview{T}" /> class.
    /// </summary>
    /// <param name="layerProperty">The layer property to apply the preview value to.</param>
    /// <param name="time">The time in the timeline at which the preview is applied.</param>
    public LayerPropertyPreview(LayerProperty<T> layerProperty, TimeSpan time)
    {
        Property = layerProperty;
        Time = time;
        OriginalKeyframe = layerProperty.Keyframes.FirstOrDefault(k => k.Position == time);
        OriginalValue = OriginalKeyframe != null ? OriginalKeyframe.Value : layerProperty.CurrentValue;
        PreviewValue = OriginalValue;
    }

    /// <summary>
    ///     Gets the property this preview applies to.
    /// </summary>
    public LayerProperty<T> Property { get; }

    /// <summary>
    ///     Gets the original keyframe of the property at the time the preview was created.
    /// </summary>
    public LayerPropertyKeyframe<T>? OriginalKeyframe { get; }

    /// <summary>
    ///     Gets the original value of the property at the time the preview was created.
    /// </summary>
    public T OriginalValue { get; }

    /// <summary>
    ///     Gets the time in the timeline at which the preview is applied.
    /// </summary>
    public TimeSpan Time { get; }

    /// <summary>
    ///     Gets the keyframe that was created to preview the value.
    /// </summary>
    public LayerPropertyKeyframe<T>? PreviewKeyframe { get; private set; }

    /// <summary>
    ///     Gets the preview value.
    /// </summary>
    public T? PreviewValue { get; private set; }

    /// <summary>
    ///     Updates the layer property to the given <paramref name="value" />, keeping track of the original state of the
    ///     property.
    /// </summary>
    /// <param name="value">The value to preview.</param>
    public void Preview(T value)
    {
        PreviewValue = value;
        PreviewKeyframe = Property.SetCurrentValue(value, Time);
    }

    /// <summary>
    ///     Discard the preview value and restores the original state of the property. The returned boolean can be used to
    ///     determine whether the preview value was different from the original value.
    /// </summary>
    /// <returns><see langword="true" /> if any changes where discarded; otherwise <see langword="false" />.</returns>
    public bool DiscardPreview()
    {
        if (PreviewKeyframe != null && OriginalKeyframe == null)
        {
            Property.RemoveKeyframe(PreviewKeyframe);
            return true;
        }

        Property.SetCurrentValue(OriginalValue, Time);
        return !Equals(OriginalValue, PreviewValue);
        ;
    }

    /// <summary>
    ///     Discard the preview value and restores the original state of the property. The returned boolean can be used to
    ///     determine whether the preview value was different from the original value.
    /// </summary>
    public void Dispose()
    {
        DiscardPreview();
    }
}
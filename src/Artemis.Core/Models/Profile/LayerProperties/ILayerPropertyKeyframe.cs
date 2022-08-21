using System;
using System.ComponentModel;
using Artemis.Storage.Entities.Profile;

namespace Artemis.Core;

/// <summary>
///     Represents a keyframe on a <see cref="ILayerProperty" /> containing a value and a timestamp
/// </summary>
public interface ILayerPropertyKeyframe : INotifyPropertyChanged
{
    /// <summary>
    ///     Gets an untyped reference to the layer property of this keyframe
    /// </summary>
    ILayerProperty UntypedLayerProperty { get; }

    /// <summary>
    ///     Gets or sets the position of this keyframe in the timeline
    /// </summary>
    TimeSpan Position { get; set; }

    /// <summary>
    ///     Gets or sets the easing function applied on the value of the keyframe
    /// </summary>
    Easings.Functions EasingFunction { get; set; }

    /// <summary>
    ///     Gets the entity this keyframe uses for persistent storage
    /// </summary>
    KeyframeEntity GetKeyframeEntity();

    /// <summary>
    ///     Removes the keyframe from the layer property
    /// </summary>
    void Remove();

    /// <summary>
    ///     Creates a copy of this keyframe.
    ///     <para>Note: The copied keyframe is not added to the layer property.</para>
    /// </summary>
    /// <returns>The resulting copy</returns>
    ILayerPropertyKeyframe CreateCopy();
}
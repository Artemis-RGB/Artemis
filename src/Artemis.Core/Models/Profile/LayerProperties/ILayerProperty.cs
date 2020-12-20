using System;
using System.Collections.Generic;
using Artemis.Storage.Entities.Profile;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents a property on a layer. Properties are saved in storage and can optionally be modified from the UI.
    ///     <para>
    ///         Note: You cannot initialize layer properties yourself. If properly placed and annotated, the Artemis core will
    ///         initialize these for you.
    ///     </para>
    /// </summary>
    public interface ILayerProperty : IStorageModel, IDisposable
    {
        /// <summary>
        ///     Gets the description attribute applied to this property
        /// </summary>
        PropertyDescriptionAttribute PropertyDescription { get; }

        /// <summary>
        ///     The parent group of this layer property, set after construction
        /// </summary>
        LayerPropertyGroup LayerPropertyGroup { get; }

        /// <summary>
        ///     Gets the unique path of the property on the layer
        /// </summary>
        string Path { get; }

        /// <summary>
        /// Gets the type of the property
        /// </summary>
        Type PropertyType { get;  }

        /// <summary>
        ///     Initializes the layer property
        ///     <para>
        ///         Note: This isn't done in the constructor to keep it parameterless which is easier for implementations of
        ///         <see cref="LayerProperty{T}" />
        ///     </para>
        /// </summary>
        void Initialize(RenderProfileElement profileElement, LayerPropertyGroup group, PropertyEntity entity, bool fromStorage, PropertyDescriptionAttribute description, string path);

        /// <summary>
        ///     Returns a list off all data binding registrations
        /// </summary>
        List<IDataBindingRegistration> GetAllDataBindingRegistrations();

        /// <summary>
        ///     Attempts to load and add the provided keyframe entity to the layer property
        /// </summary>
        /// <param name="keyframeEntity">The entity representing the keyframe to add</param>
        /// <returns>If succeeded the resulting keyframe, otherwise <see langword="null" /></returns>
        ILayerPropertyKeyframe? AddKeyframeEntity(KeyframeEntity keyframeEntity);

        /// <summary>
        ///     Updates the layer properties internal state
        /// </summary>
        /// <param name="timeline">The timeline to apply to the property</param>
        void Update(Timeline timeline);
    }
}
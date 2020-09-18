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
    public interface ILayerProperty : IStorageModel, IUpdateModel, IDisposable
    {
        /// <summary>
        ///     Gets the description attribute applied to this property
        /// </summary>
        public PropertyDescriptionAttribute PropertyDescription { get; }

        /// <summary>
        /// Gets the unique path of the property on the layer
        /// </summary>
        public string Path { get; }

        /// <summary>
        ///     Initializes the layer property
        ///     <para>
        ///         Note: This isn't done in the constructor to keep it parameterless which is easier for implementations of
        ///         <see cref="LayerProperty{T}" />
        ///     </para>
        /// </summary>
        void Initialize(RenderProfileElement profileElement, LayerPropertyGroup @group, PropertyEntity entity, bool fromStorage, PropertyDescriptionAttribute description, string path);

        /// <summary>
        ///     Returns a list off all data binding registrations
        /// </summary>
        List<IDataBindingRegistration> GetAllDataBindingRegistrations();
    }
}
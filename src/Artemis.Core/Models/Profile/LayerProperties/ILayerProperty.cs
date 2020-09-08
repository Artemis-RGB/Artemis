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
    public interface ILayerProperty : IStorageModel, IUpdateModel
    {
        /// <summary>
        ///     Initializes the layer property
        ///     <para>
        ///         Note: This isn't done in the constructor to keep it parameterless which is easier for implementations of
        ///         <see cref="LayerProperty{T}" />
        ///     </para>
        /// </summary>
        void Initialize(RenderProfileElement profileElement, LayerPropertyGroup group, PropertyEntity entity, bool fromStorage, PropertyDescriptionAttribute description);
    }
}
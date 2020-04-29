using Artemis.Storage.Entities.Profile;

namespace Artemis.Core.Models.Profile.LayerProperties
{
    public abstract class BaseLayerProperty
    {
        /// <summary>
        ///     Used to declare that this property doesn't belong to a plugin and should use the core plugin GUID
        /// </summary>
        internal bool IsCoreProperty { get; set; }

        /// <summary>
        ///     Applies the provided property entity to the layer property by deserializing the JSON base value and keyframe values
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="layerPropertyGroup"></param>
        internal abstract void ApplyToLayerProperty(PropertyEntity entity, LayerPropertyGroup layerPropertyGroup);

        /// <summary>
        ///     Saves the property to the underlying property entity that was configured when calling
        ///     <see cref="ApplyToLayerProperty" />
        /// </summary>
        internal abstract void ApplyToEntity();
    }
}
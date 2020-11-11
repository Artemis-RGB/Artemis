using Artemis.Core.LayerBrushes;

namespace Artemis.Core
{
    /// <summary>
    ///     A reference to a <see cref="LayerBrushDescriptor" />
    /// </summary>
    public class LayerBrushReference
    {
        public LayerBrushReference()
        {
        }

        public LayerBrushReference(LayerBrushDescriptor descriptor)
        {
            LayerBrushProviderId = descriptor.Provider.Id;
            BrushType = descriptor.LayerBrushType.Name;
        }

        /// <summary>
        ///     The ID of the layer brush provided the brush was provided by
        /// </summary>
        public string LayerBrushProviderId { get; set; }

        /// <summary>
        ///     The full type name of the brush descriptor
        /// </summary>
        public string BrushType { get; set; }
    }
}
using Artemis.Core.ProfileElements;
using RGB.NET.Core;

namespace Artemis.Core.Plugins.Interfaces
{
    /// <inheritdoc />
    /// <summary>
    ///     Allows you to create your own layer type
    /// </summary>
    public interface ILayerType : IPlugin
    {
        /// <summary>
        ///     Updates the layer type
        /// </summary>
        /// <param name="layer"></param>
        void Update(Layer layer);

        /// <summary>
        ///     Renders the layer type
        /// </summary>
        void Render(Layer device, RGBSurface surface);
    }
}
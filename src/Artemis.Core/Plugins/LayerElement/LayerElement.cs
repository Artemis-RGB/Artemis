using System.Drawing;
using Artemis.Core.Models.Profile;
using Artemis.Core.Models.Surface;

namespace Artemis.Core.Plugins.LayerElement
{
    public abstract class LayerElement
    {
        protected LayerElement(Layer layer, LayerElementSettings settings, LayerElementDescriptor descriptor)
        {
            Layer = layer;
            Settings = settings;
            Descriptor = descriptor;
        }

        public Layer Layer { get; }
        public LayerElementSettings Settings { get; }
        public LayerElementDescriptor Descriptor { get; }

        /// <summary>
        ///     Called by the profile editor to populate the layer element properties panel
        /// </summary>
        /// <returns></returns>
        public abstract LayerElementViewModel GetViewModel();

        /// <summary>
        ///     Called before rendering every frame, write your update logic here
        /// </summary>
        /// <param name="deltaTime"></param>
        public abstract void Update(double deltaTime);

        /// <summary>
        ///     Called before rendering, in the order configured on the layer
        /// </summary>
        public abstract void RenderPreProcess(ArtemisSurface surface, Graphics graphics);

        /// <summary>
        ///     Called during rendering, in the order configured on the layer
        /// </summary>
        public abstract void Render(ArtemisSurface surface, Graphics graphics);

        /// <summary>
        ///     Called after rendering, in the order configured on the layer
        /// </summary>
        public abstract void RenderPostProcess(ArtemisSurface surface, Graphics graphics);
    }
}
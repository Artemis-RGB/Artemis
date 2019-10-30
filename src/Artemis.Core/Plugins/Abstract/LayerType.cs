using System.Drawing;
using Artemis.Core.Models.Profile;
using Artemis.Core.Models.Surface;
using Artemis.Core.Plugins.Models;
using RGB.NET.Core;

namespace Artemis.Core.Plugins.Abstract
{
    /// <inheritdoc />
    /// <summary>
    ///     Allows you to create your own layer type
    /// </summary>
    public abstract class LayerType : Plugin
    {
        protected LayerType(PluginInfo pluginInfo) : base(pluginInfo)
        {
        }

        /// <summary>
        ///     Updates the layer type
        /// </summary>
        /// <param name="layer"></param>
        public abstract void Update(Layer layer);

        /// <summary>
        ///     Renders the layer type
        /// </summary>
        public abstract void Render(Layer device, Surface surface, Graphics graphics);
    }
}
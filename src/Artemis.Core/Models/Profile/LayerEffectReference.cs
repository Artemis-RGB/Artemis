using System;
using Artemis.Core.Plugins.LayerEffect;

namespace Artemis.Core.Models.Profile
{
    /// <summary>
    ///     A reference to a <see cref="LayerEffectDescriptor" />
    /// </summary>
    public class LayerEffectReference
    {
        /// <summary>
        ///     The GUID of the plugin the effect descriptor resides in
        /// </summary>
        public Guid EffectPluginGuid { get; set; }

        /// <summary>
        ///     The full type name of the effect descriptor
        /// </summary>
        public string EffectType { get; set; }
    }
}
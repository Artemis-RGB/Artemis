using System;
using System.Collections.Generic;
using System.Text;
using Artemis.Core.Plugins.Models;

namespace Artemis.Core.Plugins.Abstract
{
    /// <inheritdoc />
    /// <summary>
    ///     Allows you to create one or more <see cref="LayerEffect" />s usable by profile layers.
    /// </summary>
    public class LayerEffectProvider : Plugin
    {
        public LayerEffectProvider(PluginInfo pluginInfo) : base(pluginInfo)
        {
        }

        public override void EnablePlugin()
        {
            throw new NotImplementedException();
        }

        public override void DisablePlugin()
        {
            throw new NotImplementedException();
        }
    }
}

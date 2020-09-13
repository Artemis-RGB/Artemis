using Artemis.Core.LayerEffects;

namespace Artemis.Core
{
    /// <summary>
    /// An empty plugin used by <see cref="Constants.CorePluginInfo"/>
    /// </summary>
    internal class CorePlugin : Plugin
    {
        public CorePlugin()
        {
            Constants.CorePluginInfo.Instance = this;
            Enabled = true;
        }

        public override void EnablePlugin()
        {
        }

        public override void DisablePlugin()
        {
        }
    }

    internal class EffectPlaceholderPlugin : LayerEffectProvider
    {
        public EffectPlaceholderPlugin()
        {
            Enabled = true;
        }

        public override void EnablePlugin()
        {
        }

        public override void DisablePlugin()
        {
        }
    }
}
using Artemis.Core.LayerEffects;

namespace Artemis.Core
{
    /// <summary>
    /// An empty plugin used by <see cref="Constants.CorePluginInfo"/>
    /// </summary>
    internal class CorePluginImplementation : PluginImplementation
    {
        public CorePluginImplementation()
        {
            Constants.CorePluginInfo.Instance = this;
            IsEnabled = true;
        }

        public override void Enable()
        {
        }

        public override void Disable()
        {
        }
    }

    internal class EffectPlaceholderPlugin : LayerEffectProvider
    {
        public EffectPlaceholderPlugin()
        {
            IsEnabled = true;
        }

        public override void Enable()
        {
        }

        public override void Disable()
        {
        }
    }
}
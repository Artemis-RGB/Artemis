using Artemis.Core.LayerEffects;
using Artemis.Core.Modules;

namespace Artemis.Core
{
    /// <summary>
    ///     An empty data model plugin feature used by <see cref="Constants.CorePlugin" />
    /// </summary>
    internal class CorePluginFeature : Module
    {
        public CorePluginFeature()
        {
            Constants.CorePlugin.AddFeature(new PluginFeatureInfo(Constants.CorePlugin, null, this));
            IsEnabled = true;
        }

        public override void Enable()
        {
        }

        public override void Disable()
        {
        }

        public override void Update(double deltaTime)
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
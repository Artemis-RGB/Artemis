using Artemis.Core.LayerEffects;

namespace Artemis.Core
{
    /// <summary>
    ///     An empty data model plugin feature used by <see cref="Constants.CorePlugin" />
    /// </summary>
    internal class CorePluginFeature : DataModelPluginFeature
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
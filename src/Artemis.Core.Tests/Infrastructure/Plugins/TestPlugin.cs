using Artemis.Storage.Entities.Plugins;
using DryIoc;

namespace Artemis.Core.Tests.Infrastructure.Plugins;

/// <summary>
///     A plugin that hosts test features without going through plugin loading.
/// </summary>
public sealed class TestPlugin : IDisposable
{
    private readonly List<PluginFeature> _features = [];

    public TestPlugin()
    {
        PluginInfo pluginInfo = new() {Guid = Guid.NewGuid(), Name = "Artemis.Core.Tests", Version = "1.0.0", Main = "Artemis.Core.Tests.dll"};
        Plugin = new Plugin(pluginInfo, new DirectoryInfo(AppContext.BaseDirectory), new PluginEntity {PluginGuid = pluginInfo.Guid}, false)
        {
            // Same rules as ArtemisBootstrapper and PluginManagementService
            Container = new Container(rules => rules.WithConcreteTypeDynamicRegistrations().WithoutThrowOnRegisteringDisposableTransient())
        };
    }

    /// <summary>
    ///     Gets the underlying Artemis plugin
    /// </summary>
    public Plugin Plugin { get; }

    /// <summary>
    ///     Adds the feature to the plugin and enables it.
    /// </summary>
    public T AddFeature<T>(T feature) where T : PluginFeature
    {
        PluginFeatureInfo featureInfo = new(Plugin, null, feature);
        feature.Plugin = Plugin;
        feature.Info = featureInfo;
        feature.Profiler = Plugin.GetProfiler(typeof(T).Name);
        Plugin.AddFeature(featureInfo);

        // Bypass SetEnabled, it creates lock files in the plugin folder
        feature.IsEnabled = true;
        feature.Enable();
        _features.Add(feature);
        return feature;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        foreach (PluginFeature feature in _features)
        {
            feature.Disable();
            feature.IsEnabled = false;
        }

        // Brush and effect registrations live in static stores, remove ours so tests don't leak into each other
        foreach (LayerBrushRegistration registration in LayerBrushStore.GetAll().Where(r => r.PluginFeature.Plugin == Plugin))
            LayerBrushStore.Remove(registration);
        foreach (LayerEffectRegistration registration in LayerEffectStore.GetAll().Where(r => r.PluginFeature.Plugin == Plugin))
            LayerEffectStore.Remove(registration);

        Plugin.Container?.Dispose();
    }
}

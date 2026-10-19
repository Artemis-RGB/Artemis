using System.Reflection;
using Artemis.Storage.Entities.Plugins;
using DryIoc;

namespace Artemis.Core.Tests.Infrastructure.Plugins;

/// <summary>
///     A plugin that hosts test features without going through plugin loading.
///     <para>
///         Features are enabled and disabled the way PluginFeature.SetEnabled does it, minus the lock files it writes to the
///         plugin folder. Modules get their data model registered the way the data model service does it.
///     </para>
/// </summary>
public sealed class TestPlugin : IDisposable
{
    private static readonly MethodInfo OnEnabled = typeof(PluginFeature).GetMethod("OnEnabled", BindingFlags.NonPublic | BindingFlags.Instance)!;
    private static readonly MethodInfo OnDisabled = typeof(PluginFeature).GetMethod("OnDisabled", BindingFlags.NonPublic | BindingFlags.Instance)!;

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

        feature.IsEnabled = true;
        feature.InternalEnable();
        OnEnabled.Invoke(feature, null);
        _features.Add(feature);

        // Mirrors DataModelService, which registers the data model of a module once it is enabled
        if (feature is Core.Modules.Module {InternalDataModel: not null} module)
        {
            module.InternalDataModel.IsExpansion = module.IsAlwaysAvailable;
            DataModelStore.Add(module.InternalDataModel);
        }

        return feature;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        // Raising Disabled makes every registration (brushes, effects, data models, node types) remove itself from its store
        foreach (PluginFeature feature in _features)
        {
            feature.InternalDisable();
            feature.IsEnabled = false;
            OnDisabled.Invoke(feature, null);
        }

        Plugin.Container?.Dispose();
    }
}

using System.Reflection;
using DryIoc;
using Artemis.Core.Services;
using Artemis.Core.Tests.Infrastructure.Plugins;
using Artemis.Core.Tests.Infrastructure.Plugins.DeviceProviders;
using RGB.NET.Layout;

namespace Artemis.Core.Tests.Infrastructure.Services;

/// <summary>
///     Base class for tests on Artemis.Core services. Gives every test a fresh <see cref="ServiceHarness" /> and a
///     <see cref="TestPlugin" /> that the plugin management service knows about, as if Artemis had loaded it from the
///     plugins folder.
///     <para>
///         Features are enabled through the plugin management service with <see cref="EnableFeature{T}" />, the way
///         Artemis enables them, so services listening for enabled and disabled features see them. When the test ends the
///         harness unloads the plugin like Artemis does on shutdown, which disables those features again.
///     </para>
/// </summary>
public abstract class ServiceTestBase : IDisposable
{
    private static readonly FieldInfo PluginsField = typeof(PluginManagementService).GetField("_plugins", BindingFlags.NonPublic | BindingFlags.Instance)!;
    private static readonly FieldInfo ActivationTimerField = typeof(ModuleService).GetField("_activationUpdateTimer", BindingFlags.NonPublic | BindingFlags.Instance)!;
    private static readonly MethodInfo RenderScaleHandler = typeof(DeviceService).GetMethod("RenderScaleOnRenderScaleMultiplierChanged", BindingFlags.NonPublic | BindingFlags.Instance)!;

    private IModuleService? _moduleService;
    private bool _disposed;

    protected ServiceTestBase()
    {
        Services = new ServiceHarness();
        TestPlugin = new TestPlugin();

        // Plugin loading reads the plugins folder, put the test plugin straight into the list of loaded plugins instead
        TestPlugin.Plugin.SetEnabled(true);
        List<Plugin> plugins = (List<Plugin>) PluginsField.GetValue(PluginManagement)!;
        lock (plugins)
        {
            plugins.Add(TestPlugin.Plugin);
        }
    }

    protected ServiceHarness Services { get; }
    protected TestPlugin TestPlugin { get; }

    protected IPluginManagementService PluginManagement => Services.Resolve<IPluginManagementService>();

    protected T Resolve<T>() where T : notnull
    {
        return Services.Resolve<T>();
    }

    /// <summary>
    ///     Adds the feature to the test plugin and enables it through the plugin management service, like Artemis does when
    ///     a plugin is enabled. Services that listen for enabled features pick it up.
    /// </summary>
    protected T EnableFeature<T>(T feature) where T : PluginFeature
    {
        // Mirrors what PluginManagementService.EnablePlugin sets up before enabling a feature
        PluginFeatureInfo featureInfo = new(TestPlugin.Plugin, null, feature);
        feature.Info = featureInfo;
        feature.Plugin = TestPlugin.Plugin;
        feature.Profiler = TestPlugin.Plugin.GetProfiler("Feature - " + featureInfo.Name);
        feature.Entity = featureInfo.Entity;
        TestPlugin.Plugin.AddFeature(featureInfo);

        // Note: enabling briefly creates a lock file next to the test assembly, the plugin's folder
        PluginManagement.EnablePluginFeature(feature, false, false);
        return feature;
    }

    /// <summary>
    ///     Disables the feature through the plugin management service, like a user disabling it in the settings does.
    /// </summary>
    protected void DisableFeature(PluginFeature feature)
    {
        PluginManagement.DisablePluginFeature(feature, false);
    }

    /// <summary>
    ///     Creates fake devices from layout files and adds them through the device service, the way device provider
    ///     plugins add their devices when they are enabled.
    /// </summary>
    /// <param name="layoutPaths">The layouts of the devices, one device per layout, see <see cref="TestLayouts" /></param>
    /// <returns>The device provider, its devices are on <see cref="IDeviceService.Devices" /></returns>
    protected TestDeviceProvider AddDeviceProvider(params string[] layoutPaths)
    {
        return AddDeviceProvider(new TestDeviceProvider(), layoutPaths);
    }

    /// <summary>
    ///     Enables the device provider and adds devices created from layout files through the device service.
    ///     <para>
    ///         Note: features of the same type share their ID, which the device service uses to tell providers apart, use
    ///         a different type of provider for each provider in a test.
    ///     </para>
    /// </summary>
    protected T AddDeviceProvider<T>(T deviceProvider, params string[] layoutPaths) where T : TestDeviceProvider
    {
        EnableFeature(deviceProvider);
        foreach (string layoutPath in layoutPaths)
        {
            DeviceLayout layout = DeviceLayout.Load(layoutPath) ?? throw new ArgumentException($"Failed to load layout {layoutPath}", nameof(layoutPaths));
            deviceProvider.CreateDevice(layout, (_, _) => { });
        }

        // The device service initializes the provider again, which recreates the devices from the same layouts
        Resolve<IDeviceService>().AddDeviceProvider(deviceProvider);
        return deviceProvider;
    }

    /// <summary>
    ///     Creates a new profile service on the same database, like starting Artemis again
    /// </summary>
    protected IProfileService CreateNewProfileService()
    {
        return Services.Container.New<ProfileService>();
    }

    /// <summary>
    ///     Gets the module service with its 2 second activation timer stopped, tests call
    ///     <see cref="IModuleService.UpdateModuleActivation" /> themselves. Use this rather than <see cref="Resolve{T}" />.
    /// </summary>
    protected IModuleService ResolveModuleService()
    {
        _moduleService ??= Resolve<IModuleService>();
        ((System.Timers.Timer) ActivationTimerField.GetValue(_moduleService)!).Stop();
        return _moduleService;
    }

    public virtual void Dispose()
    {
        if (_disposed)
            return;
        _disposed = true;

        // Neither service is disposable, stop what would otherwise outlive the test. The module service's timer keeps
        // running and the device service stays subscribed to the static render scale event
        if (_moduleService != null)
            ((System.Timers.Timer) ActivationTimerField.GetValue(_moduleService)!).Dispose();
        RenderScale.RenderScaleMultiplierChanged -= (EventHandler) Delegate.CreateDelegate(typeof(EventHandler), Resolve<IDeviceService>(), RenderScaleHandler);

        // Disposing the plugin management service unloads its plugins, which forces two full garbage collections per
        // plugin. Disable the test plugin's features the way unloading does and take the plugin out of the list instead
        foreach (PluginFeatureInfo featureInfo in TestPlugin.Plugin.Features)
        {
            if (featureInfo.Instance is {IsEnabled: true} feature)
                PluginManagement.DisablePluginFeature(feature, false);
        }

        List<Plugin> plugins = (List<Plugin>) PluginsField.GetValue(PluginManagement)!;
        lock (plugins)
        {
            plugins.Remove(TestPlugin.Plugin);
        }

        Services.Dispose();
        TestPlugin.Dispose();
    }
}

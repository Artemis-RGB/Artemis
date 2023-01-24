using System;
using System.Threading.Tasks;
using System.Timers;
using Artemis.Core.Modules;
using Artemis.Core.Services;
using DryIoc;
using Serilog;

namespace Artemis.Core;

/// <summary>
///     Represents a registration for a timed plugin update
/// </summary>
public sealed class TimedUpdateRegistration : IDisposable
{
    private readonly object _lock = new();
    private readonly ILogger _logger;
    private bool _disposed;
    private DateTime _lastEvent;
    private Timer? _timer;

    internal TimedUpdateRegistration(PluginFeature feature, TimeSpan interval, Action<double> action, string? name)
    {
        if (CoreService.Container == null)
            throw new ArtemisCoreException("Cannot create a TimedUpdateRegistration before initializing the Core");
        _logger = CoreService.Container.Resolve<ILogger>();

        Feature = feature;
        Interval = interval;
        Action = action;
        Name = name ?? $"TimedUpdate-{Guid.NewGuid().ToString().Substring(0, 8)}";

        Feature.Enabled += FeatureOnEnabled;
        Feature.Disabled += FeatureOnDisabled;
        if (Feature.IsEnabled)
            Start();
    }

    internal TimedUpdateRegistration(PluginFeature feature, TimeSpan interval, Func<double, Task> asyncAction, string? name)
    {
        if (CoreService.Container == null)
            throw new ArtemisCoreException("Cannot create a TimedUpdateRegistration before initializing the Core");
        _logger = CoreService.Container.Resolve<ILogger>();

        Feature = feature;
        Interval = interval;
        AsyncAction = asyncAction;
        Name = name ?? $"TimedUpdate-{Guid.NewGuid().ToString().Substring(0, 8)}";

        Feature.Enabled += FeatureOnEnabled;
        Feature.Disabled += FeatureOnDisabled;
        if (Feature.IsEnabled)
            Start();
    }


    /// <summary>
    ///     Gets the plugin feature this registration is associated with
    /// </summary>
    public PluginFeature Feature { get; }

    /// <summary>
    ///     Gets the interval at which the update should occur
    /// </summary>
    public TimeSpan Interval { get; }

    /// <summary>
    ///     Gets the action that gets called each time the update event fires
    /// </summary>
    public Action<double>? Action { get; }

    /// <summary>
    ///     Gets the task that gets called each time the update event fires
    /// </summary>
    public Func<double, Task>? AsyncAction { get; }

    /// <summary>
    ///     Gets the name of this timed update
    /// </summary>
    public string Name { get; }

    /// <summary>
    ///     Starts calling the <see cref="Action" /> or <see cref="AsyncAction" /> at the configured <see cref="Interval" />
    ///     <para>Note: Called automatically when the plugin enables</para>
    /// </summary>
    public void Start()
    {
        if (_disposed)
            throw new ObjectDisposedException("TimedUpdateRegistration");

        lock (_lock)
        {
            if (!Feature.IsEnabled)
                throw new ArtemisPluginException("Cannot start a timed update for a disabled plugin feature");

            if (_timer != null)
                return;

            _lastEvent = DateTime.Now;
            _timer = new Timer(Interval.TotalMilliseconds);
            _timer.Elapsed += TimerOnElapsed;
            _timer.Start();
        }
    }

    /// <summary>
    ///     Stops calling the <see cref="Action" /> or <see cref="AsyncAction" /> at the configured <see cref="Interval" />
    ///     <para>Note: Called automatically when the plugin disables</para>
    /// </summary>
    public void Stop()
    {
        if (_disposed)
            throw new ObjectDisposedException("TimedUpdateRegistration");

        lock (_lock)
        {
            if (_timer == null)
                return;

            _timer.Elapsed -= TimerOnElapsed;
            _timer.Stop();
            _timer.Dispose();
            _timer = null;
        }
    }

    /// <inheritdoc />
    public override string ToString()
    {
        if (Interval.TotalSeconds >= 1)
            return $"{Name} ({Interval.TotalSeconds} sec)";
        return $"{Name} ({Interval.TotalMilliseconds} ms)";
    }

    private void TimerOnElapsed(object? sender, ElapsedEventArgs e)
    {
        if (!Feature.IsEnabled)
            return;

        lock (_lock)
        {
            Feature.Profiler.StartMeasurement(ToString());

            TimeSpan interval = DateTime.Now - _lastEvent;
            _lastEvent = DateTime.Now;

            // Modules don't always want to update, honor that
            if (Feature is Module module && !module.IsUpdateAllowed)
                return;

            try
            {
                if (Action != null)
                {
                    Action(interval.TotalSeconds);
                }
                else if (AsyncAction != null)
                {
                    Task task = AsyncAction(interval.TotalSeconds);
                    task.Wait();
                }
            }
            catch (Exception exception)
            {
                _logger.Error(exception, "{timedUpdate} uncaught exception in plugin {plugin}", this, Feature.Plugin);
            }
            finally
            {
                Feature.Profiler.StopMeasurement(ToString());
            }
        }
    }

    private void FeatureOnEnabled(object? sender, EventArgs e)
    {
        Start();
    }

    private void FeatureOnDisabled(object? sender, EventArgs e)
    {
        Stop();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Stop();

        Feature.Enabled -= FeatureOnEnabled;
        Feature.Disabled -= FeatureOnDisabled;

        _disposed = true;
        Feature.Profiler.ClearMeasurements(ToString());
    }
}
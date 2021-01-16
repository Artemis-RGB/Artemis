using System;
using System.Threading.Tasks;
using System.Timers;
using Artemis.Core.Modules;
using Artemis.Core.Services;
using Ninject;
using Serilog;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents a registration for a timed plugin update
    /// </summary>
    public class TimedUpdateRegistration : IDisposable
    {
        private DateTime _lastEvent;
        private Timer? _timer;
        private bool _disposed;
        private readonly object _lock = new();
        private ILogger _logger;

        internal TimedUpdateRegistration(PluginFeature feature, TimeSpan interval, Action<double> action)
        {
            _logger = CoreService.Kernel.Get<ILogger>();

            Feature = feature;
            Interval = interval;
            Action = action;

            Feature.Enabled += FeatureOnEnabled;
            Feature.Disabled += FeatureOnDisabled;
            if (Feature.IsEnabled)
                Start();
        }

        internal TimedUpdateRegistration(PluginFeature feature, TimeSpan interval, Func<double, Task> asyncAction)
        {
            _logger = CoreService.Kernel.Get<ILogger>();
            
            Feature = feature;
            Interval = interval;
            AsyncAction = asyncAction;

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
        ///     Starts calling the <see cref="Action" /> or <see cref="AsyncAction"/> at the configured <see cref="Interval" />
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
        ///     Stops calling the <see cref="Action" /> or <see cref="AsyncAction"/> at the configured <see cref="Interval" />
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

        private void TimerOnElapsed(object? sender, ElapsedEventArgs e)
        {
            if (!Feature.IsEnabled)
                return;

            lock (_lock)
            {
                TimeSpan interval = DateTime.Now - _lastEvent;
                _lastEvent = DateTime.Now;

                // Modules don't always want to update, honor that
                if (Feature is Module module && !module.IsUpdateAllowed)
                    return;

                try
                {
                    if (Action != null)
                        Action(interval.TotalSeconds);
                    else if (AsyncAction != null)
                    {
                        Task task = AsyncAction(interval.TotalSeconds);
                        task.Wait();
                    }
                }
                catch (Exception exception)
                {
                    _logger.Error(exception, "Timed update uncaught exception in plugin {plugin}", Feature.Plugin);
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

        #region IDisposable

        /// <summary>
        ///     Releases the unmanaged resources used by the object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">
        ///     <see langword="true" /> to release both managed and unmanaged resources;
        ///     <see langword="false" /> to release only unmanaged resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Stop();

                Feature.Enabled -= FeatureOnEnabled;
                Feature.Disabled -= FeatureOnDisabled;

                _disposed = true;
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
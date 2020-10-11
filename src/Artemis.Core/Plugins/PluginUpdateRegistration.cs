using System;
using System.Threading.Tasks;
using System.Timers;
using Artemis.Core.Modules;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents a registration for a timed plugin update
    /// </summary>
    public class PluginUpdateRegistration
    {
        private DateTime _lastEvent;
        private Timer _timer;

        internal PluginUpdateRegistration(PluginInfo pluginInfo, TimeSpan interval, Action<double> action)
        {
            PluginInfo = pluginInfo;
            Interval = interval;
            Action = action;

            PluginInfo.Instance.PluginEnabled += InstanceOnPluginEnabled;
            PluginInfo.Instance.PluginDisabled += InstanceOnPluginDisabled;
            if (PluginInfo.Instance.Enabled)
                Start();
        }

        internal PluginUpdateRegistration(PluginInfo pluginInfo, TimeSpan interval, Func<double, Task> asyncAction)
        {
            PluginInfo = pluginInfo;
            Interval = interval;
            AsyncAction = asyncAction;
            
            PluginInfo.Instance.PluginEnabled += InstanceOnPluginEnabled;
            PluginInfo.Instance.PluginDisabled += InstanceOnPluginDisabled;
            if (PluginInfo.Instance.Enabled)
                Start();
        }


        /// <summary>
        ///     Gets the plugin info of the plugin this registration is associated with
        /// </summary>
        public PluginInfo PluginInfo { get; }

        /// <summary>
        ///     Gets the interval at which the update should occur
        /// </summary>
        public TimeSpan Interval { get; }

        /// <summary>
        ///     Gets the action that gets called each time the update event fires
        /// </summary>
        public Action<double> Action { get; }

        /// <summary>
        ///     Gets the task that gets called each time the update event fires
        /// </summary>
        public Func<double, Task> AsyncAction { get; }

        /// <summary>
        ///     Starts calling the <see cref="Action" /> or <see cref="AsyncAction"/> at the configured <see cref="Interval" />
        ///     <para>Note: Called automatically when the plugin enables</para>
        /// </summary>
        public void Start()
        {
            lock (this)
            {
                if (!PluginInfo.Instance.Enabled)
                    throw new ArtemisPluginException("Cannot start a timed update for a disabled plugin");

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
            lock (this)
            {
                if (_timer == null)
                    return;

                _timer.Elapsed -= TimerOnElapsed;
                _timer.Stop();
                _timer.Dispose();
                _timer = null;
            }
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            if (!PluginInfo.Instance.Enabled)
                return;

            TimeSpan interval = DateTime.Now - _lastEvent;
            _lastEvent = DateTime.Now;

            // Modules don't always want to update, honor that
            if (PluginInfo.Instance is Module module && !module.IsUpdateAllowed)
                return;

            if (Action != null)
                Action(interval.TotalSeconds);
            else if (AsyncAction != null)
            {
                Task task = AsyncAction(interval.TotalSeconds);
                task.Wait();
            }
        }

        private void InstanceOnPluginEnabled(object sender, EventArgs e)
        {
            Start();
        }

        private void InstanceOnPluginDisabled(object sender, EventArgs e)
        {
            Stop();
        }
    }
}
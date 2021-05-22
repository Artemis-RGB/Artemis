using System.Linq;
using Artemis.Core;
using Artemis.UI.Shared;
using Stylet;

namespace Artemis.UI.Screens.Settings.Debug.Tabs.Performance
{
    public class PerformanceDebugPluginViewModel : Screen
    {
        public Plugin Plugin { get; }
        public object Icon { get; }

        public PerformanceDebugPluginViewModel(Plugin plugin)
        {
            Plugin = plugin;
            Icon = PluginUtilities.GetPluginIcon(Plugin, Plugin.Info.Icon);
        }

        public BindableCollection<PerformanceDebugProfilerViewModel> Profilers { get; } = new();
        public void Update()
        {
            foreach (Profiler pluginProfiler in Plugin.Profilers.Where(p => p.Measurements.Any()))
            {
                if (Profilers.All(p => p.Profiler != pluginProfiler))
                    Profilers.Add(new PerformanceDebugProfilerViewModel(pluginProfiler));
            }

            foreach (PerformanceDebugProfilerViewModel profilerViewModel in Profilers)
                profilerViewModel.Update();
        }
    }
}
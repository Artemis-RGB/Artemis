using System.Collections.ObjectModel;
using System.Linq;
using Artemis.Core;
using Artemis.UI.Shared;

namespace Artemis.UI.Screens.Debugger.Performance
{
    public class PerformanceDebugPluginViewModel : ViewModelBase
    {
        public PerformanceDebugPluginViewModel(Plugin plugin)
        {
            Plugin = plugin;
        }

        public Plugin Plugin { get; }

        public ObservableCollection<PerformanceDebugProfilerViewModel> Profilers { get; } = new();

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
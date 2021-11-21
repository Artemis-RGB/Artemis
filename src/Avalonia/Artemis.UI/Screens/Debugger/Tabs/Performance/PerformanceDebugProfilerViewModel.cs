using System.Collections.ObjectModel;
using System.Linq;
using Artemis.Core;
using Artemis.UI.Shared;

namespace Artemis.UI.Screens.Debugger.Tabs.Performance
{
    public class PerformanceDebugProfilerViewModel : ViewModelBase
    {
        public PerformanceDebugProfilerViewModel(Profiler profiler)
        {
            Profiler = profiler;
        }

        public Profiler Profiler { get; }

        public ObservableCollection<PerformanceDebugMeasurementViewModel> Measurements { get; } = new();

        public void Update()
        {
            foreach ((string _, ProfilingMeasurement measurement) in Profiler.Measurements)
            {
                if (Measurements.All(m => m.Measurement != measurement))
                    Measurements.Add(new PerformanceDebugMeasurementViewModel(measurement));
            }

            foreach (PerformanceDebugMeasurementViewModel profilingMeasurementViewModel in Measurements)
                profilingMeasurementViewModel.Update();
        }
    }
}
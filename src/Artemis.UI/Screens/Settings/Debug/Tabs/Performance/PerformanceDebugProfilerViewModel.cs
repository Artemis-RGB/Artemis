using System.Linq;
using Artemis.Core;
using Stylet;

namespace Artemis.UI.Screens.Settings.Debug.Tabs.Performance
{
    public class PerformanceDebugProfilerViewModel : Screen
    {
        public Profiler Profiler { get; }

        public PerformanceDebugProfilerViewModel(Profiler profiler)
        {
            Profiler = profiler;
        }
        
        public BindableCollection<PerformanceDebugMeasurementViewModel> Measurements { get; } = new();

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
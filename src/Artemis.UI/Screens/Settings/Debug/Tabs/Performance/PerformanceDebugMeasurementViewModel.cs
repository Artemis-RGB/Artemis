using Artemis.Core;
using Stylet;

namespace Artemis.UI.Screens.Settings.Debug.Tabs.Performance
{
    public class PerformanceDebugMeasurementViewModel : PropertyChangedBase
    {
        private string _average;
        private string _last;
        private string _max;
        private string _min;
        private string _percentile;

        public PerformanceDebugMeasurementViewModel(ProfilingMeasurement measurement)
        {
            Measurement = measurement;
        }

        public ProfilingMeasurement Measurement { get; }

        public string Last
        {
            get => _last;
            set => SetAndNotify(ref _last, value);
        }

        public string Average
        {
            get => _average;
            set => SetAndNotify(ref _average, value);
        }

        public string Min
        {
            get => _min;
            set => SetAndNotify(ref _min, value);
        }

        public string Max
        {
            get => _max;
            set => SetAndNotify(ref _max, value);
        }

        public string Percentile
        {
            get => _percentile;
            set => SetAndNotify(ref _percentile, value);
        }

        public void Update()
        {
            Last = Measurement.GetLast().TotalMilliseconds + " ms";
            Average = Measurement.GetAverage().TotalMilliseconds + " ms";
            Min = Measurement.GetMin().TotalMilliseconds + " ms";
            Max = Measurement.GetMax().TotalMilliseconds + " ms";
            Percentile = Measurement.GetPercentile(0.95).TotalMilliseconds + " ms";
        }
    }
}
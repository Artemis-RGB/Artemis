using Artemis.Core;
using Artemis.UI.Shared;
using PropertyChanged.SourceGenerator;

namespace Artemis.UI.Screens.Debugger.Performance;

public partial class PerformanceDebugMeasurementViewModel : ViewModelBase
{
    [Notify] private string? _average;
    [Notify] private string? _last;
    [Notify] private string? _max;
    [Notify] private string? _min;
    [Notify] private string? _percentile;
    [Notify] private string? _count;

    public PerformanceDebugMeasurementViewModel(ProfilingMeasurement measurement)
    {
        Measurement = measurement;
    }

    public ProfilingMeasurement Measurement { get; }

    public void Update()
    {
        Last = Measurement.GetLast().TotalMilliseconds + " ms";
        Average = Measurement.GetAverage().TotalMilliseconds + " ms";
        Min = Measurement.GetMin().TotalMilliseconds + " ms";
        Max = Measurement.GetMax().TotalMilliseconds + " ms";
        Percentile = Measurement.GetPercentile(0.95).TotalMilliseconds + " ms";
        Count = Measurement.GetCount().ToString();
    }
}
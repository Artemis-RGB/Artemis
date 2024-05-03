using System.Collections.Generic;
using System.Diagnostics;

namespace Artemis.Core;

/// <summary>
///     Represents a profiler that can measure time between calls distinguished by identifiers
/// </summary>
public class Profiler
{
    internal Profiler(Plugin plugin, string name)
    {
        Plugin = plugin;
        Name = name;
    }

    /// <summary>
    ///     Gets the plugin this profiler belongs to
    /// </summary>
    public Plugin Plugin { get; }

    /// <summary>
    ///     Gets the name of this profiler
    /// </summary>
    public string Name { get; }
    
    /// <summary>
    ///     Gets a dictionary containing measurements by their identifiers
    /// </summary>
    public Dictionary<string, ProfilingMeasurement> Measurements { get; set; } = new();

    /// <summary>
    ///     Starts measuring time for the provided <paramref name="identifier" />
    /// </summary>
    /// <param name="identifier">A unique identifier for this measurement</param>
    public void StartMeasurement(string identifier)
    {
        lock (Measurements)
        {
            if (!Measurements.TryGetValue(identifier, out ProfilingMeasurement? measurement))
            {
                measurement = new ProfilingMeasurement(identifier);
                Measurements.Add(identifier, measurement);
            }

            measurement.Start();
        }
    }

    /// <summary>
    ///     Stops measuring time for the provided <paramref name="identifier" />
    /// </summary>
    /// <param name="identifier">A unique identifier for this measurement</param>
    /// <returns>The number of ticks that passed since the <see cref="StartMeasurement" /> call with the same identifier</returns>
    public long StopMeasurement(string identifier)
    {
        long lockRequestedAt = Stopwatch.GetTimestamp();
        lock (Measurements)
        {
            if (!Measurements.TryGetValue(identifier, out ProfilingMeasurement? measurement))
            {
                measurement = new ProfilingMeasurement(identifier);
                Measurements.Add(identifier, measurement);
            }

            return measurement.Stop(Stopwatch.GetTimestamp() - lockRequestedAt);
        }
    }

    /// <summary>
    ///     Clears measurements with the the provided <paramref name="identifier" />
    /// </summary>
    /// <param name="identifier"></param>
    public void ClearMeasurements(string identifier)
    {
        lock (Measurements)
        {
            Measurements.Remove(identifier);
        }
    }
}
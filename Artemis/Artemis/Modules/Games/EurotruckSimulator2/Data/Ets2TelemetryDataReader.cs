using System;
using Artemis.Modules.Games.EurotruckSimulator2.Data.Reader;

namespace Artemis.Modules.Games.EurotruckSimulator2.Data
{
    public class Ets2TelemetryDataReader : IDisposable
    {
        /// <summary>
        ///     ETS2 telemetry plugin maps the data to this mapped file name.
        /// </summary>
        private const string Ets2TelemetryMappedFileName = "Local\\Ets2TelemetryServer";

        // ReSharper disable once InconsistentNaming
        private static readonly Lazy<Ets2TelemetryDataReader> instance = new Lazy<Ets2TelemetryDataReader>(
            () => new Ets2TelemetryDataReader());

        private readonly Ets2TelemetryData _data = new Ets2TelemetryData();

        private readonly object _lock = new object();

        private readonly SharedProcessMemory<Ets2TelemetryStructure> _sharedMemory =
            new SharedProcessMemory<Ets2TelemetryStructure>(Ets2TelemetryMappedFileName);

        public static Ets2TelemetryDataReader Instance => instance.Value;

        public bool IsConnected => _sharedMemory.IsConnected;

        public void Dispose()
        {
            _sharedMemory?.Dispose();
        }

        public IEts2TelemetryData Read()
        {
            lock (_lock)
            {
                _sharedMemory.Data = default(Ets2TelemetryStructure);
                _sharedMemory.Read();
                _data.Update(_sharedMemory.Data);
                return _data;
            }
        }
    }
}
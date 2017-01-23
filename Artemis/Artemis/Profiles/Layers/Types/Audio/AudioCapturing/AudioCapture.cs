using System;
using System.Linq;
using System.Timers;
using CSCore;
using CSCore.CoreAudioAPI;
using CSCore.DSP;
using CSCore.SoundIn;
using CSCore.SoundOut;
using CSCore.Streams;
using Ninject.Extensions.Logging;

namespace Artemis.Profiles.Layers.Types.Audio.AudioCapturing
{
    public class AudioCapture
    {
        private const FftSize FftSize = CSCore.DSP.FftSize.Fft1024;
        private readonly Timer _volumeTimer;
        private readonly double[] _volumeValues;
        private readonly Timer _disableTimer;
        private bool _mayStop;
        private SingleSpectrum _singleSpectrum;
        private ISoundIn _soundIn;
        private GainSource _source;
        private BasicSpectrumProvider _spectrumProvider;
        private GainSource _volume;
        private int _volumeIndex;

        public AudioCapture(ILogger logger, MMDevice device, MmDeviceType type)
        {
            Logger = logger;
            Device = device;
            Type = type;
            DesiredAverage = 0.75;

            _volumeValues = new double[5];
            _volumeIndex = 0;
            _disableTimer = new Timer(1000);
            _disableTimer.Elapsed += CheckStop;
            _volumeTimer = new Timer(200);
            _volumeTimer.Elapsed += VolumeTimerOnElapsed;
        }

        public ILogger Logger { get; }
        public MMDevice Device { get; }
        public MmDeviceType Type { get; }
        public double DesiredAverage { get; set; }

        public float Volume
        {
            get { return _volume.Volume; }
            set { _volume.Volume = value; }
        }

        public bool Running { get; set; }

        private void VolumeTimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            if (Volume <= 0)
                Volume = 1;

            var currentValue = _singleSpectrum.GetValue();
            if (currentValue == null)
                return;

            _volumeValues[_volumeIndex] = currentValue.Value;

            if (_volumeIndex == 4)
            {
                _volumeIndex = 0;
            }
            else
            {
                _volumeIndex++;
                return;
            }

            var averageVolume = _volumeValues.Average();
            // Don't adjust when there is virtually no audio
            if (averageVolume < 0.01)
                return;
            // Don't bother when the volume with within a certain marigin
            if (averageVolume > DesiredAverage - 0.1 && averageVolume < DesiredAverage + 0.1)
                return;

            if (averageVolume < DesiredAverage && Volume < 50)
            {
                Logger.Trace("averageVolume:{0} | DesiredAverage:{1} | Volume:{2} so increase.", currentValue,
                    DesiredAverage, Volume);
                Volume++;
            }
            else if (Volume > 1)
            {
                Logger.Trace("averageVolume:{0} | DesiredAverage:{1} | Volume:{2} so decrease.", currentValue,
                    DesiredAverage, Volume);
                Volume--;
            }
        }

        public LineSpectrum GetLineSpectrum(int barCount, ScalingStrategy scalingStrategy)
        {
            if (_spectrumProvider == null)
                return null;
            return new LineSpectrum(FftSize)
            {
                SpectrumProvider = _spectrumProvider,
                UseAverage = true,
                BarCount = barCount,
                IsXLogScale = true,
                ScalingStrategy = scalingStrategy
            };
        }

        /// <summary>
        ///     Keeps the audio capture active, when not called for longer than 1 sec the capture will
        ///     stop capturing until Pulse is called again
        /// </summary>
        public void Pulse()
        {
            _mayStop = false;
            if (!Running)
                Start();
        }

        private void CheckStop(object sender, ElapsedEventArgs e)
        {
            if (_mayStop)
            {
                Logger.Debug("Stopping idle audio capture for device: {0}", Device?.FriendlyName ?? "default");
                Stop();
            }
            else
                _mayStop = true;
        }

        private void Start()
        {
            Logger.Debug("Starting audio capture for device: {0}", Device?.FriendlyName ?? "default");

            try
            {
                Stop();

                if (Type == MmDeviceType.Input)
                {
                    _soundIn = Device != null
                        ? new WasapiCapture {Device = Device}
                        : new WasapiCapture();
                }
                else
                {
                    _soundIn = Device != null
                        ? new WasapiLoopbackCapture {Device = Device}
                        : new WasapiLoopbackCapture();
                }

                _soundIn.Initialize();

                var soundInSource = new SoundInSource(_soundIn);
                _source = soundInSource.ToSampleSource().AppendSource(x => new GainSource(x), out _volume);

                // create a spectrum provider which provides fft data based on some input
                _spectrumProvider = new BasicSpectrumProvider(_source.WaveFormat.Channels, _source.WaveFormat.SampleRate,
                    FftSize);

                // the SingleBlockNotificationStream is used to intercept the played samples
                var notificationSource = new SingleBlockNotificationStream(_source);
                // pass the intercepted samples as input data to the spectrumprovider (which will calculate a fft based on them)
                notificationSource.SingleBlockRead += (s, a) => _spectrumProvider.Add(a.Left, a.Right);

                var waveSource = notificationSource.ToWaveSource(16);
                // We need to read from our source otherwise SingleBlockRead is never called and our spectrum provider is not populated
                var buffer = new byte[waveSource.WaveFormat.BytesPerSecond / 2];
                soundInSource.DataAvailable += (s, aEvent) =>
                {
                    while (waveSource.Read(buffer, 0, buffer.Length) > 0)
                    {
                    }
                };

                _singleSpectrum = new SingleSpectrum(FftSize, _spectrumProvider);
                _mayStop = false;

                _disableTimer.Start();
                _volumeTimer.Start();
                _soundIn.Start();

                Running = true;
            }
            catch (Exception e)
            {
                Logger.Warn(e, "Failed to start WASAPI audio capture");
            }
        }

        private void Stop()
        {
            Running = false;


            if (_soundIn != null)
            {
                _soundIn.Stop();
                _soundIn.Dispose();
                _soundIn = null;
            }
            if (_source != null)
            {
                _source.Dispose();
                _source = null;
            }

            _disableTimer.Stop();
            _volumeTimer.Stop();
        }
    }
}
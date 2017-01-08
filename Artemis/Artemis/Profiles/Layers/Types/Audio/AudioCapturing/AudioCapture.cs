using System;
using System.Timers;
using CSCore;
using CSCore.CoreAudioAPI;
using CSCore.DSP;
using CSCore.SoundIn;
using CSCore.Streams;
using Ninject.Extensions.Logging;

namespace Artemis.Profiles.Layers.Types.Audio.AudioCapturing
{
    public class AudioCapture
    {
        private const FftSize FftSize = CSCore.DSP.FftSize.Fft4096;
        private WasapiLoopbackCapture _soundIn;
        private GainSource _source;
        private BasicSpectrumProvider _spectrumProvider;
        private GainSource _volume;
        private Timer _timer;

        public AudioCapture(ILogger logger, MMDevice device)
        {
            Logger = logger;
            Device = device;

            _timer = new Timer(1000);
            _timer.Elapsed += TimerOnElapsed;
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            // If MayStop is true for longer than a second, this will stop the audio capture
            if (MayStop)
            {
                Stop();
                MayStop = false;
            }
            else
                MayStop = true;
        }

        public bool MayStop { get; set; }

        public ILogger Logger { get; }

        public float Volume
        {
            get { return _volume.Volume; }
            set { _volume.Volume = value; }
        }

        public MMDevice Device { get; }
        public bool Running { get; set; }

        public LineSpectrum GetLineSpectrum(int barCount, int volume, ScalingStrategy scalingStrategy)
        {
            return new LineSpectrum(FftSize)
            {
                SpectrumProvider = _spectrumProvider,
                UseAverage = true,
                BarCount = barCount,
                IsXLogScale = true,
                ScalingStrategy = scalingStrategy
            };
        }

        public void Start()
        {
            if (Running)
                return;

            try
            {
                _soundIn = new WasapiLoopbackCapture();
                _soundIn.Initialize();
                // Not sure if this null check is needed but doesnt hurt
                if (Device != null)
                    _soundIn.Device = Device;

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

                _soundIn.Start();
                _timer.Start();
                Running = true;
                MayStop = false;
            }
            catch (Exception e)
            {
                Logger.Warn(e, "Failed to start WASAPI audio capture");
            }
        }

        public void Stop()
        {
            if (!Running)
                return;

            try
            {
                _timer.Stop();
                _soundIn.Stop();
                _soundIn.Dispose();
                _source.Dispose();
                _soundIn = null;
                _source = null;

                Running = false;
            }
            catch (Exception e)
            {
                Logger.Warn(e, "Failed to stop WASAPI audio capture");
            }
        }
    }
}
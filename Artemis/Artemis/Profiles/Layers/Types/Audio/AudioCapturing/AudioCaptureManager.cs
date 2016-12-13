using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.Modules.Effects.AudioVisualizer.Utilities;
using NAudio.CoreAudioApi;
using NAudio.Dsp;
using NAudio.Wave;
using Ninject.Extensions.Logging;

namespace Artemis.Profiles.Layers.Types.Audio.AudioCapturing
{
    public class AudioCaptureManager
    {
        private readonly SampleAggregator _sampleAggregator = new SampleAggregator(1024);
        private readonly WasapiLoopbackCapture _waveIn;
        private Complex[] _fft;
        private DateTime _lastAudioUpdate;
        private DateTime _lastRequest;

        public AudioCaptureManager(ILogger logger)
        {
            Logger = logger;
            Device = new MMDeviceEnumerator().EnumerateAudioEndPoints(DataFlow.All, DeviceState.Active).FirstOrDefault();

            _sampleAggregator.FftCalculated += FftCalculated;
            _sampleAggregator.PerformFFT = true;

            // Start listening for sound data
            _waveIn = new WasapiLoopbackCapture();
            _waveIn.DataAvailable += OnDataAvailable;
        }

        public ILogger Logger { get; set; }
        public MMDevice Device { get; set; }

        public bool Running { get; set; }

        public void Start()
        {
            if (Running)
                return;

            try
            {
                _waveIn.StartRecording();
                Running = true;
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
                _waveIn.StopRecording();
                Running = false;
            }
            catch (Exception e)
            {
                Logger.Warn(e, "Failed to start WASAPI audio capture");
            }
        }

        private void FftCalculated(object sender, FftEventArgs e)
        {
            _fft = e.Result;
        }

        private void OnDataAvailable(object sender, WaveInEventArgs e)
        {
            if (DateTime.Now - _lastAudioUpdate < TimeSpan.FromMilliseconds(40))
                return;
            if (DateTime.Now - _lastRequest > TimeSpan.FromSeconds(5))
            {
                Stop();
                return;
            }

            _lastAudioUpdate = DateTime.Now;

            var buffer = e.Buffer;
            var bytesRecorded = e.BytesRecorded;
            var bufferIncrement = _waveIn.WaveFormat.BlockAlign;

            for (var index = 0; index < bytesRecorded; index += bufferIncrement)
            {
                var sample32 = BitConverter.ToSingle(buffer, index);
                _sampleAggregator.Add(sample32);
            }
        }

        public List<byte> GetSpectrumData(int lines)
        {
            _lastRequest = DateTime.Now;
            if (!Running)
                Start();

            var spectrumData = new List<byte>();

            if (_fft == null)
                return spectrumData;

            int x;
            var b0 = 0;

            for (x = 0; x < lines; x++)
            {
                float peak = 0;
                var b1 = (int) Math.Pow(2, x*10.0/(lines - 1));
                if (b1 > 1023)
                    b1 = 1023;
                if (b1 <= b0)
                    b1 = b0 + 1;
                for (; b0 < b1; b0++)
                    if (peak < _fft[1 + b0].X)
                        peak = _fft[1 + b0].X;
                var y = (int) (Math.Sqrt(peak)*3*255 - 4);
                if (y > 255)
                    y = 255;
                if (y < 0)
                    y = 0;
                spectrumData.Add((byte) y);
            }

            return spectrumData;
        }
    }
}
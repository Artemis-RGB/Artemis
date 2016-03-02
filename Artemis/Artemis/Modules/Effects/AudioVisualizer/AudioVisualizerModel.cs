using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using Artemis.Managers;
using Artemis.Models;
using Artemis.Utilities;
using Artemis.Utilities.Audio;
using Artemis.Utilities.Keyboard;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace Artemis.Modules.Effects.AudioVisualizer
{
    public class AudioVisualizerModel : EffectModel
    {
        private const int FftLength = 2048;
        private readonly SampleAggregator _sampleAggregator = new SampleAggregator(FftLength);
        private bool _fromBottom;
        private bool _generating;
        private int _sensitivity;
        private IWaveIn _waveIn;

        public AudioVisualizerModel(MainManager mainManager, AudioVisualizerSettings settings) : base(mainManager)
        {
            Settings = settings;
            Name = "Audiovisualizer";
            DeviceIds = new List<string>();
            SpectrumData = new List<byte>();
            Scale = 4;
            Initialized = false;
        }

        public int Lines { get; set; }

        public int Scale { get; set; }

        public AudioVisualizerSettings Settings { get; set; }
        public List<byte> SpectrumData { get; set; }
        public List<KeyboardRectangle> SoundRectangles { get; set; }

        public List<string> DeviceIds { get; set; }
        public string SelectedDeviceId { get; set; }

        public override void Dispose()
        {
            Initialized = false;
            _sampleAggregator.PerformFFT = false;
            _sampleAggregator.FftCalculated -= FftCalculated;

            _waveIn.StopRecording();
            _waveIn.DataAvailable -= OnDataAvailable;
            _waveIn = null;
        }

        public override void Enable()
        {
            Initialized = false;
            Lines = MainManager.KeyboardManager.ActiveKeyboard.Width;

            // TODO: Device selection
            SelectedDeviceId = new MMDeviceEnumerator()
                .EnumerateAudioEndPoints(DataFlow.All, DeviceState.Active)
                .FirstOrDefault()?.ID;

            // Apply settings
            SoundRectangles = new List<KeyboardRectangle>();
            for (var i = 0; i < Lines; i++)
            {
                SoundRectangles.Add(new KeyboardRectangle(
                    MainManager.KeyboardManager.ActiveKeyboard,
                    0, 0, new List<Color>
                    {
                        ColorHelpers.ToDrawingColor(Settings.TopColor),
                        ColorHelpers.ToDrawingColor(Settings.MiddleColor),
                        ColorHelpers.ToDrawingColor(Settings.BottomColor)
                    },
                    LinearGradientMode.Vertical) {ContainedBrush = false, Height = 0});
            }
            _sensitivity = Settings.Sensitivity;
            _fromBottom = Settings.FromBottom;
            _sampleAggregator.FftCalculated += FftCalculated;
            _sampleAggregator.PerformFFT = true;

            // Start listening for sound data
            _waveIn = new WasapiLoopbackCapture();
            _waveIn.DataAvailable += OnDataAvailable;
            _waveIn.StartRecording();

            Initialized = true;
        }

        public override void Update()
        {
            // Start filling the model
            _generating = true;

            if (SelectedDeviceId == null)
                return;

            var device = new MMDeviceEnumerator()
                .EnumerateAudioEndPoints(DataFlow.All, DeviceState.Active)
                .FirstOrDefault(d => d.ID == SelectedDeviceId);

            if (device == null || SpectrumData == null)
                return;
            if (!SpectrumData.Any())
                return;

            // Parse spectrum data
            for (var i = 0; i < Lines; i++)
            {
                int height;
                if (SpectrumData.Count - 1 < i || SpectrumData[i] == 0)
                    height = 0;
                else
                    height = (int) Math.Round(SpectrumData[i]/2.55);

                // Apply Sensitivity setting
                height = height*_sensitivity;
                var keyboardHeight =
                    (int) Math.Round(MainManager.KeyboardManager.ActiveKeyboard.Height/100.00*height*Scale);
                if (keyboardHeight > SoundRectangles[i].Height)
                    SoundRectangles[i].Height = keyboardHeight;
                else
                    SoundRectangles[i].Height = SoundRectangles[i].Height - Settings.FadeSpeed;
                // Apply Bars setting
                SoundRectangles[i].X = i*Scale;
                SoundRectangles[i].Width = Scale;

                if (_fromBottom)
                    SoundRectangles[i].Y = MainManager.KeyboardManager.ActiveKeyboard.Height*Scale -
                                           SoundRectangles[i].Height;
            }
            _generating = false;
        }

        public override Bitmap GenerateBitmap()
        {
            if (SpectrumData == null || SoundRectangles == null)
                return null;

            // Lock the _spectrumData array while busy with it
            _generating = true;

            var bitmap = MainManager.KeyboardManager.ActiveKeyboard.KeyboardBitmap(Scale);
            using (var g = Graphics.FromImage(bitmap))
            {
                foreach (var soundRectangle in SoundRectangles)
                    soundRectangle.Draw(g);
            }

            _generating = false;
            return bitmap;
        }

        private void OnDataAvailable(object sender, WaveInEventArgs e)
        {
            var buffer = e.Buffer;
            var bytesRecorded = e.BytesRecorded;
            var bufferIncrement = _waveIn.WaveFormat.BlockAlign;

            for (var index = 0; index < bytesRecorded; index += bufferIncrement)
            {
                var sample32 = BitConverter.ToSingle(buffer, index);
                _sampleAggregator.Add(sample32);
            }
        }

        private void FftCalculated(object sender, FftEventArgs e)
        {
            if (_generating)
                return;

            int x;
            var b0 = 0;

            SpectrumData.Clear();
            for (x = 0; x < Lines; x++)
            {
                float peak = 0;
                var b1 = (int) Math.Pow(2, x*10.0/(Lines - 1));
                if (b1 > 2047)
                    b1 = 2047;
                if (b1 <= b0)
                    b1 = b0 + 1;
                for (; b0 < b1; b0++)
                {
                    if (peak < e.Result[1 + b0].X)
                        peak = e.Result[1 + b0].X;
                }
                var y = (int) (Math.Sqrt(peak)*3*255 - 4);
                if (y > 255)
                    y = 255;
                if (y < 0)
                    y = 0;
                SpectrumData.Add((byte) y);
            }
        }
    }
}
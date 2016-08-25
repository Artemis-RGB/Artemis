using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Artemis.Managers;
using Artemis.Models.Interfaces;
using Artemis.Modules.Effects.AudioVisualizer.Utilities;
using Artemis.Profiles.Layers.Abstract;
using Artemis.Profiles.Layers.Animations;
using Artemis.Profiles.Layers.Interfaces;
using Artemis.Profiles.Layers.Models;
using Artemis.Properties;
using Artemis.Utilities;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using Newtonsoft.Json;

namespace Artemis.Profiles.Layers.Types.Audio
{
    internal class AudioType : ILayerType
    {
        private readonly List<LayerModel> _audioLayers = new List<LayerModel>();
        private readonly MMDevice _device;
        private readonly SampleAggregator _sampleAggregator = new SampleAggregator(2048);
        private readonly WasapiLoopbackCapture _waveIn;
        private int _lines;
        private AudioPropertiesModel _previousSettings;

        public AudioType(MainManager mainManager)
        {
            _device =
                new MMDeviceEnumerator().EnumerateAudioEndPoints(DataFlow.All, DeviceState.Active).FirstOrDefault();

            _sampleAggregator.FftCalculated += FftCalculated;
            _sampleAggregator.PerformFFT = true;

            // Start listening for sound data
            _waveIn = new WasapiLoopbackCapture();
            _waveIn.DataAvailable += OnDataAvailable;
            _waveIn.StartRecording();
        }

        [JsonIgnore]
        public List<byte> SpectrumData { get; set; } = new List<byte>();

        public string Name { get; } = "Keyboard - Audio visualization";
        public bool ShowInEdtor { get; } = true;
        public DrawType DrawType { get; } = DrawType.Keyboard;

        public ImageSource DrawThumbnail(LayerModel layer)
        {
            var thumbnailRect = new Rect(0, 0, 18, 18);
            var visual = new DrawingVisual();
            using (var c = visual.RenderOpen())
            {
                c.DrawImage(ImageUtilities.BitmapToBitmapImage(Resources.gif), thumbnailRect);
            }

            var image = new DrawingImage(visual.Drawing);
            return image;
        }

        public void Draw(LayerModel layer, DrawingContext c)
        {
            lock (SpectrumData)
            {
                foreach (var audioLayer in _audioLayers)
                {
                    // This is cheating but it ensures that the brush is drawn across the entire main-layer
                    var oldWidth = audioLayer.Properties.Width;
                    var oldHeight = audioLayer.Properties.Height;
                    var oldX = audioLayer.Properties.X;
                    var oldY = audioLayer.Properties.Y;

                    audioLayer.Properties.Width = layer.Properties.Width;
                    audioLayer.Properties.Height = layer.Properties.Height;
                    audioLayer.Properties.X = layer.Properties.X;
                    audioLayer.Properties.Y = layer.Properties.Y;
                    audioLayer.LayerType.Draw(audioLayer, c);

                    audioLayer.Properties.Width = oldWidth;
                    audioLayer.Properties.Height = oldHeight;
                    audioLayer.Properties.X = oldX;
                    audioLayer.Properties.Y = oldY;
                }
            }
        }

        public void Update(LayerModel layerModel, IDataModel dataModel, bool isPreview = false)
        {
            if ((_device == null) || isPreview)
                return;

            lock (SpectrumData)
            {
                UpdateLayers(layerModel);

                if (!SpectrumData.Any())
                    return;

                var settings = (AudioPropertiesModel) layerModel.Properties;
                if (settings.Direction == Direction.TopToBottom || settings.Direction == Direction.BottomToTop)
                    ApplyVertical(settings);
                else if (settings.Direction == Direction.LeftToRight || settings.Direction == Direction.RightToLeft)
                    ApplyHorizontal(settings);
            }
        }

        public void SetupProperties(LayerModel layerModel)
        {
            if (layerModel.Properties is AudioPropertiesModel)
                return;

            layerModel.Properties = new AudioPropertiesModel(layerModel.Properties)
            {
                FadeSpeed = 0.2,
                Sensitivity = 2
            };
        }

        public LayerPropertiesViewModel SetupViewModel(LayerPropertiesViewModel layerPropertiesViewModel,
            List<ILayerAnimation> layerAnimations, IDataModel dataModel, LayerModel proposedLayer)
        {
            if (layerPropertiesViewModel is AudioPropertiesViewModel)
                return layerPropertiesViewModel;
            return new AudioPropertiesViewModel(proposedLayer, dataModel);
        }

        private void ApplyVertical(AudioPropertiesModel settings)
        {
            var index = 0;
            foreach (var audioLayer in _audioLayers)
            {
                int height;
                if (SpectrumData.Count > index)
                    height = (int) Math.Round(SpectrumData[index]/2.55);
                else
                    height = 0;

                // Apply Sensitivity setting
                height = height*settings.Sensitivity;

                var newHeight = settings.Height/100.0*height;
                if (newHeight >= audioLayer.Properties.Height)
                    audioLayer.Properties.Height = newHeight;
                else
                    audioLayer.Properties.Height = audioLayer.Properties.Height - settings.FadeSpeed;
                if (audioLayer.Properties.Height < 0)
                    audioLayer.Properties.Height = 0;

                // Reverse the direction if settings require it
                if (settings.Direction == Direction.BottomToTop)
                    audioLayer.Properties.Y = settings.Y + (settings.Height - audioLayer.Properties.Height);

                audioLayer.Update(null, false, true);
                index++;
            }
        }

        private void ApplyHorizontal(AudioPropertiesModel settings)
        {
            var index = 0;
            foreach (var audioLayer in _audioLayers)
            {
                int width;
                if (SpectrumData.Count > index)
                    width = (int) Math.Round(SpectrumData[index]/2.55);
                else
                    width = 0;

                // Apply Sensitivity setting
                width = width*settings.Sensitivity;

                var newWidth = settings.Width/100.0*width;
                if (newWidth >= audioLayer.Properties.Width)
                    audioLayer.Properties.Width = newWidth;
                else
                    audioLayer.Properties.Width = audioLayer.Properties.Width - settings.FadeSpeed;
                if (audioLayer.Properties.Width < 0)
                    audioLayer.Properties.Width = 0;

                audioLayer.Properties.Brush = settings.Brush;
                audioLayer.Properties.Contain = false;

                // Reverse the direction if settings require it
                if (settings.Direction == Direction.RightToLeft)
                    audioLayer.Properties.X = settings.X + (settings.Width - audioLayer.Properties.Width);

                audioLayer.Update(null, false, true);
                index++;
            }
        }

        /// <summary>
        ///     Updates the inner layers when the settings have changed
        /// </summary>
        /// <param name="layerModel"></param>
        private void UpdateLayers(LayerModel layerModel)
        {
            var settings = (AudioPropertiesModel) layerModel.Properties;
            if (JsonConvert.SerializeObject(settings).Equals(JsonConvert.SerializeObject(_previousSettings)))
                return;

            _previousSettings = GeneralHelpers.Clone((AudioPropertiesModel) layerModel.Properties);

            _audioLayers.Clear();
            if (settings.Direction == Direction.TopToBottom || settings.Direction == Direction.BottomToTop)
                SetupVertical(settings);
            else if (settings.Direction == Direction.LeftToRight || settings.Direction == Direction.RightToLeft)
                SetupHorizontal(settings);
        }

        private void SetupVertical(AudioPropertiesModel settings)
        {
            _lines = (int) settings.Width;
            for (var i = 0; i < _lines; i++)
            {
                var layer = LayerModel.CreateLayer();
                layer.Properties.X = settings.X + i;
                layer.Properties.Y = settings.Y;
                layer.Properties.Width = 1;
                layer.Properties.Height = 0;
                layer.LayerAnimation = new NoneAnimation();
                layer.Properties.Brush = settings.Brush;
                layer.Properties.Contain = false;

                _audioLayers.Add(layer);
                layer.Update(null, false, true);
            }
        }

        private void SetupHorizontal(AudioPropertiesModel settings)
        {
            _lines = (int) settings.Height;
            for (var i = 0; i < _lines; i++)
            {
                var layer = LayerModel.CreateLayer();
                layer.Properties.X = settings.X;
                layer.Properties.Y = settings.Y + i;
                layer.Properties.Width = 0;
                layer.Properties.Height = 1;
                layer.LayerAnimation = new NoneAnimation();
                layer.Properties.Brush = settings.Brush;
                layer.Properties.Contain = false;

                _audioLayers.Add(layer);
                layer.Update(null, false, true);
            }
        }


        private void FftCalculated(object sender, FftEventArgs e)
        {
            if (_lines == 0)
                return;

            lock (SpectrumData)
            {
                int x;
                var b0 = 0;

                SpectrumData.Clear();
                for (x = 0; x < _lines; x++)
                {
                    float peak = 0;
                    var b1 = (int) Math.Pow(2, x*10.0/(_lines - 1));
                    if (b1 > 2047)
                        b1 = 2047;
                    if (b1 <= b0)
                        b1 = b0 + 1;
                    for (; b0 < b1; b0++)
                        if (peak < e.Result[1 + b0].X)
                            peak = e.Result[1 + b0].X;
                    var y = (int) (Math.Sqrt(peak)*3*255 - 4);
                    if (y > 255)
                        y = 255;
                    if (y < 0)
                        y = 0;
                    SpectrumData.Add((byte) y);
                }
            }
        }

        // TODO: Check how often this is called
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
    }
}
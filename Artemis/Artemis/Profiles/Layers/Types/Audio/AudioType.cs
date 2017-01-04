using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Artemis.Modules.Abstract;
using Artemis.Profiles.Layers.Abstract;
using Artemis.Profiles.Layers.Interfaces;
using Artemis.Profiles.Layers.Models;
using Artemis.Profiles.Layers.Types.Audio.AudioCapturing;
using Artemis.Properties;
using Artemis.Utilities;
using Artemis.ViewModels.Profiles;
using Newtonsoft.Json;
using Ninject;

namespace Artemis.Profiles.Layers.Types.Audio
{
    public class AudioType : ILayerType
    {
        private readonly List<LayerModel> _audioLayers = new List<LayerModel>();
        private readonly IKernel _kernel;

        private DateTime _lastUpdate;
        private int _lines;
        private string _previousSettings;

        public AudioType(IKernel kernel, AudioCaptureManager audioCaptureManager)
        {
            _kernel = kernel;
            AudioCaptureManager = audioCaptureManager;
        }

        [JsonIgnore]
        public AudioCaptureManager AudioCaptureManager { get; set; }

        public string Name => "Keyboard - Audio visualization";
        public bool ShowInEdtor => true;
        public DrawType DrawType => DrawType.Keyboard;

        public ImageSource DrawThumbnail(LayerModel layer)
        {
            var thumbnailRect = new Rect(0, 0, 18, 18);
            var visual = new DrawingVisual();
            using (var c = visual.RenderOpen())
            {
                c.DrawImage(ImageUtilities.BitmapToBitmapImage(Resources.audio), thumbnailRect);
            }

            var image = new DrawingImage(visual.Drawing);
            return image;
        }

        public void Draw(LayerModel layerModel, DrawingContext c)
        {
            lock (_audioLayers)
            {
                foreach (var audioLayer in _audioLayers)
                {
                    // This is cheating but it ensures that the brush is drawn across the entire main-layer
                    var oldWidth = audioLayer.Properties.Width;
                    var oldHeight = audioLayer.Properties.Height;
                    var oldX = audioLayer.Properties.X;
                    var oldY = audioLayer.Properties.Y;

                    audioLayer.Properties.Width = layerModel.Properties.Width;
                    audioLayer.Properties.Height = layerModel.Properties.Height;
                    audioLayer.Properties.X = layerModel.Properties.X;
                    audioLayer.Properties.Y = layerModel.Properties.Y;
                    audioLayer.LayerType.Draw(audioLayer, c);

                    audioLayer.Properties.Width = oldWidth;
                    audioLayer.Properties.Height = oldHeight;
                    audioLayer.Properties.X = oldX;
                    audioLayer.Properties.Y = oldY;
                }
            }
        }

        public void Update(LayerModel layerModel, ModuleDataModel dataModel, bool isPreview = false)
        {
            layerModel.ApplyProperties(true);
            if (isPreview)
                return;

            lock (_audioLayers)
            {
                SetupLayers(layerModel);
                var spectrumData = AudioCaptureManager.GetSpectrumData(_lines);
                if (!spectrumData.Any())
                    return;

                var settings = (AudioPropertiesModel) layerModel.Properties;
                switch (settings.Direction)
                {
                    case Direction.TopToBottom:
                    case Direction.BottomToTop:
                        ApplyVertical(spectrumData, settings);
                        break;
                    case Direction.LeftToRight:
                    case Direction.RightToLeft:
                        ApplyHorizontal(spectrumData, settings);
                        break;
                }
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

        public LayerPropertiesViewModel SetupViewModel(LayerEditorViewModel layerEditorViewModel,
            LayerPropertiesViewModel layerPropertiesViewModel)
        {
            if (layerPropertiesViewModel is AudioPropertiesViewModel)
                return layerPropertiesViewModel;
            return new AudioPropertiesViewModel(layerEditorViewModel);
        }

        private void ApplyVertical(List<byte> spectrumData, AudioPropertiesModel settings)
        {
            var index = 0;
            foreach (var audioLayer in _audioLayers)
            {
                int height;
                if (spectrumData.Count > index)
                    height = (int) Math.Round(spectrumData[index]/2.55);
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

                FakeUpdate(settings, audioLayer);
                index++;
            }
        }

        private void ApplyHorizontal(List<byte> spectrumData, AudioPropertiesModel settings)
        {
            var index = 0;
            foreach (var audioLayer in _audioLayers)
            {
                int width;
                if (spectrumData.Count > index)
                    width = (int) Math.Round(spectrumData[index]/2.55);
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

                FakeUpdate(settings, audioLayer);
                index++;
            }
        }

        /// <summary>
        ///     Updates the layer manually faking the width and height for a properly working animation
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="audioLayer"></param>
        private static void FakeUpdate(LayerPropertiesModel settings, LayerModel audioLayer)
        {
            // Call the regular update
            audioLayer.LayerType?.Update(audioLayer, null);

            // Store the original height and width
            var oldHeight = audioLayer.Properties.Height;
            var oldWidth = audioLayer.Properties.Width;

            // Fake the height and width and update the animation
            audioLayer.Properties.Width = settings.Width;
            audioLayer.Properties.Height = settings.Height;
            audioLayer.LastRender = DateTime.Now;
            audioLayer.LayerAnimation?.Update(audioLayer, true);

            // Restore the height and width
            audioLayer.Properties.Height = oldHeight;
            audioLayer.Properties.Width = oldWidth;
        }

        /// <summary>
        ///     Sets up the inner layers when the settings have changed
        /// </summary>
        /// <param name="layerModel"></param>
        private void SetupLayers(LayerModel layerModel)
        {
            // Checking on settings update is expensive, only do it every second
            if (DateTime.Now - _lastUpdate < TimeSpan.FromSeconds(1))
                return;
            _lastUpdate = DateTime.Now;

            var settings = (AudioPropertiesModel) layerModel.Properties;
            var currentSettings = JsonConvert.SerializeObject(settings, Formatting.Indented);
            var currentType = _audioLayers.FirstOrDefault()?.LayerAnimation?.GetType();

            if (currentSettings == _previousSettings && (layerModel.LayerAnimation.GetType() == currentType))
                return;

            _previousSettings = JsonConvert.SerializeObject(settings, Formatting.Indented);

            _audioLayers.Clear();
            switch (settings.Direction)
            {
                case Direction.TopToBottom:
                case Direction.BottomToTop:
                    SetupVertical(layerModel);
                    break;
                case Direction.LeftToRight:
                case Direction.RightToLeft:
                    SetupHorizontal(layerModel);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void SetupVertical(LayerModel layerModel)
        {
            _lines = (int) layerModel.Properties.Width;
            for (var i = 0; i < _lines; i++)
            {
                var layer = LayerModel.CreateLayer();
                layer.Properties.X = layerModel.Properties.X + i;
                layer.Properties.Y = layerModel.Properties.Y;
                layer.Properties.Width = 1;
                layer.Properties.Height = 0;
                layer.Properties.AnimationSpeed = layerModel.Properties.AnimationSpeed;
                layer.Properties.Brush = layerModel.Properties.Brush;
                layer.Properties.Contain = false;
                layer.LayerAnimation = (ILayerAnimation) _kernel.Get(layerModel.LayerAnimation.GetType());

                _audioLayers.Add(layer);
                layer.Update(null, false, true);
            }
        }

        private void SetupHorizontal(LayerModel layerModel)
        {
            _lines = (int) layerModel.Properties.Height;
            for (var i = 0; i < _lines; i++)
            {
                var layer = LayerModel.CreateLayer();
                layer.Properties.X = layerModel.Properties.X;
                layer.Properties.Y = layerModel.Properties.Y + i;
                layer.Properties.Width = 0;
                layer.Properties.Height = 1;
                layer.Properties.AnimationSpeed = layerModel.Properties.AnimationSpeed;
                layer.Properties.Brush = layerModel.Properties.Brush;
                layer.Properties.Contain = false;
                layer.LayerAnimation = (ILayerAnimation) _kernel.Get(layerModel.LayerAnimation.GetType());

                _audioLayers.Add(layer);
                layer.Update(null, false, true);
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Artemis.Events;
using Artemis.Managers;
using Artemis.Modules.Abstract;
using Artemis.Profiles.Layers.Abstract;
using Artemis.Profiles.Layers.Animations;
using Artemis.Profiles.Layers.Interfaces;
using Artemis.Profiles.Layers.Models;
using Artemis.Profiles.Layers.Types.Audio.AudioCapturing;
using Artemis.Properties;
using Artemis.Utilities;
using Artemis.ViewModels;
using CSCore.CoreAudioAPI;

namespace Artemis.Profiles.Layers.Types.Audio
{
    public class AudioType : ILayerType
    {
        private readonly AudioCaptureManager _audioCaptureManager;
        private AudioCapture _audioCapture;
        private int _lines;
        private LineSpectrum _lineSpectrum;
        private List<double> _lineValues;
        private AudioPropertiesModel _properties;
        private bool _subscribed;
        private DateTime _lastRender;

        public AudioType(AudioCaptureManager audioCaptureManager)
        {
            _audioCaptureManager = audioCaptureManager;
        }

        private void SubscribeToAudioChange()
        {
            if (_subscribed)
                return;

            _audioCaptureManager.AudioDeviceChanged += OnAudioDeviceChanged;
            _subscribed = true;
        }

        public string Name => "Keyboard - Audio visualization";
        public bool ShowInEdtor => true;
        public DrawType DrawType => DrawType.Keyboard;
        public int DrawScale => 4;

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
            if (_lineValues == null)
                return;

            var parentX = layerModel.X;
            var parentY = layerModel.Y;
            var direction = ((AudioPropertiesModel) layerModel.Properties).Direction;

            // Create a geometry that will be formed by all the bars
            var barGeometry = new GeometryGroup();

            switch (direction)
            {
                case Direction.TopToBottom:
                    for (var index = 0; index < _lineValues.Count; index++)
                    {
                        var clipRect = new Rect((parentX + index) * 4, parentY * 4, 4, _lineValues[index] * 4);
                        var barRect = new RectangleGeometry(clipRect);
                        barGeometry.Children.Add(barRect);
                    }
                    break;
                case Direction.BottomToTop:
                    for (var index = 0; index < _lineValues.Count; index++)
                    {
                        var clipRect = new Rect((parentX + index) * 4, parentY * 4, 4, _lineValues[index] * 4);
                        clipRect.Y = clipRect.Y + layerModel.Height * 4 - clipRect.Height;
                        var barRect = new RectangleGeometry(clipRect);
                        barGeometry.Children.Add(barRect);
                    }
                    break;
                case Direction.LeftToRight:
                    for (var index = 0; index < _lineValues.Count; index++)
                    {
                        var clipRect = new Rect((parentX + index) * 4, parentY * 4, 4, _lineValues[index] * 4);
                        var barRect = new RectangleGeometry(clipRect);
                        barGeometry.Children.Add(barRect);
                    }
                    break;
                default:
                    for (var index = 0; index < _lineValues.Count; index++)
                    {
                        var clipRect = new Rect((parentX + index) * 4, parentY * 4, 4, _lineValues[index] * 4);
                        var barRect = new RectangleGeometry(clipRect);
                        barGeometry.Children.Add(barRect);
                    }
                    break;
            }

            // Push the created geometry
            c.PushClip(barGeometry);
            BrushDraw(layerModel, c);
            c.Pop();
        }

        public void Update(LayerModel layerModel, ModuleDataModel dataModel, bool isPreview = false)
        {
            layerModel.ApplyProperties(true);
            var newProperties = (AudioPropertiesModel) layerModel.Properties;
            if (_properties == null)
                _properties = newProperties;

            SubscribeToAudioChange();

            if (_audioCapture == null || newProperties.Device != _properties.Device || newProperties.DeviceType != _properties.DeviceType)
            {
                var device = GetMmDevice();
                if (device != null)
                    _audioCapture = _audioCaptureManager.GetAudioCapture(device, newProperties.DeviceType);
            }

            _properties = newProperties;

            if (_audioCapture == null)
                return;

            _audioCapture.Pulse();

            var direction = ((AudioPropertiesModel) layerModel.Properties).Direction;

            int currentLines;
            double currentHeight;
            if (direction == Direction.BottomToTop || direction == Direction.TopToBottom)
            {
                currentLines = (int) layerModel.Width;
                currentHeight = layerModel.Height;
            }
            else
            {
                currentLines = (int) layerModel.Height;
                currentHeight = layerModel.Width;
            }

            // Get a new line spectrum if the lines changed, it is null or the layer hasn't rendered for a few frames
            if (_lines != currentLines || _lineSpectrum == null || DateTime.Now - _lastRender > TimeSpan.FromMilliseconds(100))
            {
                _lines = currentLines;
                _lineSpectrum = _audioCapture.GetLineSpectrum(_lines, ScalingStrategy.Decibel);
            }
            
            var newLineValues = _audioCapture.GetLineSpectrum(_lines, ScalingStrategy.Decibel)?.GetLineValues(currentHeight);
            if (newLineValues != null)
            {
                _lineValues = newLineValues;
                _lastRender = DateTime.Now;
            }
        }

        public void SetupProperties(LayerModel layerModel)
        {
            if (layerModel.Properties is AudioPropertiesModel)
                return;

            layerModel.Properties = new AudioPropertiesModel(layerModel.Properties)
            {
                DeviceType = MmDeviceType.Ouput,
                Device = "Default",
                Direction = Direction.BottomToTop,
                ScalingStrategy = ScalingStrategy.Decibel
            };
        }

        public LayerPropertiesViewModel SetupViewModel(LayerEditorViewModel layerEditorViewModel,
            LayerPropertiesViewModel layerPropertiesViewModel)
        {
            if (layerPropertiesViewModel is AudioPropertiesViewModel)
                return layerPropertiesViewModel;
            return new AudioPropertiesViewModel(layerEditorViewModel);
        }

        public void BrushDraw(LayerModel layerModel, DrawingContext c)
        {
            // If an animation is present, let it handle the drawing
            if (layerModel.LayerAnimation != null && !(layerModel.LayerAnimation is NoneAnimation))
            {
                layerModel.LayerAnimation.Draw(layerModel, c, DrawScale);
                return;
            }

            // Otherwise draw the rectangle with its layer.AppliedProperties dimensions and brush
            var rect = layerModel.Properties.Contain
                ? layerModel.LayerRect()
                : new Rect(layerModel.Properties.X * 4, layerModel.Properties.Y * 4,
                    layerModel.Properties.Width * 4, layerModel.Properties.Height * 4);

            var clip = layerModel.LayerRect(DrawScale);

            // Can't meddle with the original brush because it's frozen.
            var brush = layerModel.Brush.Clone();
            brush.Opacity = layerModel.Opacity;

            c.PushClip(new RectangleGeometry(clip));
            c.DrawRectangle(brush, null, rect);
            c.Pop();
        }

        private void OnAudioDeviceChanged(object sender, AudioDeviceChangedEventArgs e)
        {
            if (_properties == null || _properties.Device != "Default")
                return;

            if (_properties.DeviceType == MmDeviceType.Input)
            {
                if (e.DefaultRecording != null)
                    _audioCapture = _audioCaptureManager.GetAudioCapture(e.DefaultRecording, MmDeviceType.Input);
            }
            else
            {
                if (e.DefaultPlayback != null)
                    _audioCapture = _audioCaptureManager.GetAudioCapture(e.DefaultPlayback, MmDeviceType.Ouput);
            }

            _lines = 0;
        }

        private MMDevice GetMmDevice()
        {
            if (_properties == null)
                return null;

            if (_properties.DeviceType == MmDeviceType.Input)
                return _properties.Device == "Default"
                    ? MMDeviceEnumerator.TryGetDefaultAudioEndpoint(DataFlow.Capture, Role.Multimedia)
                    : MMDeviceEnumerator.EnumerateDevices(DataFlow.Capture)
                        .FirstOrDefault(d => d.FriendlyName == _properties.Device);
            return _properties.Device == "Default"
                ? MMDeviceEnumerator.TryGetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia)
                : MMDeviceEnumerator.EnumerateDevices(DataFlow.Render)
                    .FirstOrDefault(d => d.FriendlyName == _properties.Device);
        }
    }
}
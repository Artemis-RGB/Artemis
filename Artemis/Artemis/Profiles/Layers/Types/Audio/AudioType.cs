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

namespace Artemis.Profiles.Layers.Types.Audio
{
    public class AudioType : ILayerType
    {
        private readonly AudioCapture _audioCapture;
        private int _lines;
        private LineSpectrum _lineSpectrum;
        private Point[] _points;

        public AudioType(AudioCaptureManager audioCaptureManager)
        {
            _audioCapture = audioCaptureManager.GetAudioCapture(null);
        }

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
            var parentX = layerModel.X * 4;
            var parentY = layerModel.Y * 4;
            var pen = new Pen(layerModel.Brush, 4);
            var direction = ((AudioPropertiesModel) layerModel.Properties).Direction;
            if (direction == Direction.BottomToTop || direction == Direction.TopToBottom)
            {
                for (var index = 0; index < _points.Length; index++)
                {
                    var startPoint = new Point(index * 4 + 2 + parentX, _points[index].Y * 4 + parentY);
                    var endPoint = new Point(index * 4 + 2 + parentX, parentY);
                    var clip = new Rect(startPoint, endPoint);
                    clip.Width = 4;
                    c.PushClip(new RectangleGeometry(new Rect(startPoint, endPoint)));
                    var point = new Point(index * 4 + 2 + parentX, _points[index].Y * 4 + parentY);
                    c.DrawLine(pen, startPoint, endPoint);
                }
            }
            else
            {
                for (var index = 0; index < _points.Length; index++)
                {
                    var point = new Point(_points[index].X * 4 + parentX, index * 4 + 2 + parentY);
                    c.DrawLine(pen, point, new Point(parentX, index * 4 + 2 + parentY));
                }
            }
        }

        public void Update(LayerModel layerModel, ModuleDataModel dataModel, bool isPreview = false)
        {
            layerModel.ApplyProperties(true);

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

            if (_lines != currentLines)
            {
                _lines = currentLines;
                _points = new Point[_lines];
                _lineSpectrum = _audioCapture.GetLineSpectrum(_lines, ScalingStrategy.Decibel);
            }

            // Let audio capture know it is being listened to
            _audioCapture.Pulse();

            if (_lineSpectrum == null)
                return;

            if (direction == Direction.BottomToTop || direction == Direction.TopToBottom)
                _lineSpectrum.UpdateLinesVertical(currentHeight, _points);
            else
                _lineSpectrum.UpdateLinesHorizontal(currentHeight, _points);
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
    }
}
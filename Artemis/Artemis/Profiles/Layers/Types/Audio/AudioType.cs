using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using Artemis.Modules.Abstract;
using Artemis.Profiles.Layers.Abstract;
using Artemis.Profiles.Layers.Animations;
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
        private const GeometryCombineMode CombineMode = GeometryCombineMode.Union;
        private readonly AudioCapture _audioCapture;
        private int _lines;
        private LineSpectrum _lineSpectrum;
        private List<double> _lineValues;

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
            if (_lineValues == null)
                return;

            var parentX = layerModel.X;
            var parentY = layerModel.Y;
            var direction = ((AudioPropertiesModel) layerModel.Properties).Direction;

            // Create a geometry that will be formed by all the bars
            Geometry barGeometry = new RectangleGeometry();

            switch (direction)
            {
                case Direction.TopToBottom:
                    for (var index = 0; index < _lineValues.Count; index++)
                    {
                        var clipRect = new Rect((parentX + index)*4, parentY*4, 4, _lineValues[index]*4);
                        var barRect = new RectangleGeometry(clipRect);
                        barGeometry = Geometry.Combine(barGeometry, barRect, CombineMode, Transform.Identity);
                    }
                    break;
                case Direction.BottomToTop:
                    for (var index = 0; index < _lineValues.Count; index++)
                    {
                        var clipRect = new Rect((parentX + index)*4, parentY*4, 4, _lineValues[index]*4);
                        clipRect.Y = clipRect.Y + layerModel.Height*4 - clipRect.Height;
                        var barRect = new RectangleGeometry(clipRect);
                        barGeometry = Geometry.Combine(barGeometry, barRect, CombineMode, Transform.Identity);
                    }
                    break;
                case Direction.LeftToRight:
                    for (var index = 0; index < _lineValues.Count; index++)
                    {
                        var clipRect = new Rect((parentX + index)*4, parentY*4, 4, _lineValues[index]*4);
                        var barRect = new RectangleGeometry(clipRect);
                        barGeometry = Geometry.Combine(barGeometry, barRect, CombineMode, Transform.Identity);
                    }
                    break;
                default:
                    for (var index = 0; index < _lineValues.Count; index++)
                    {
                        var clipRect = new Rect((parentX + index)*4, parentY*4, 4, _lineValues[index]*4);
                        var barRect = new RectangleGeometry(clipRect);
                        barGeometry = Geometry.Combine(barGeometry, barRect, CombineMode, Transform.Identity);
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

            if (_lines != currentLines || _lineSpectrum == null)
            {
                _lines = currentLines;
                _lineSpectrum = _audioCapture.GetLineSpectrum(_lines, ScalingStrategy.Decibel);
            }

            var newLineValues = _lineSpectrum?.GetLineValues(currentHeight);
            if (newLineValues != null)
                _lineValues = newLineValues;
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

        public void BrushDraw(LayerModel layerModel, DrawingContext c)
        {
            // If an animation is present, let it handle the drawing
            if (layerModel.LayerAnimation != null && !(layerModel.LayerAnimation is NoneAnimation))
            {
                layerModel.LayerAnimation.Draw(layerModel, c);
                return;
            }

            // Otherwise draw the rectangle with its layer.AppliedProperties dimensions and brush
            var rect = layerModel.Properties.Contain
                ? layerModel.LayerRect()
                : new Rect(layerModel.Properties.X*4, layerModel.Properties.Y*4,
                    layerModel.Properties.Width*4, layerModel.Properties.Height*4);

            var clip = layerModel.LayerRect();

            // Can't meddle with the original brush because it's frozen.
            var brush = layerModel.Brush.Clone();
            brush.Opacity = layerModel.Opacity;

            c.PushClip(new RectangleGeometry(clip));
            c.DrawRectangle(brush, null, rect);
            c.Pop();
        }
    }
}
using Artemis.Core.Plugins.LayerElement;
using SkiaSharp;

namespace Artemis.Plugins.LayerElements.Noise
{
    public class NoiseLayerElementSettings : LayerElementSettings
    {
        private float _animationSpeed;
        private SKBlendMode _blendMode;
        private SKColor _color;
        private float _xScale;
        private float _yScale;
        
        public NoiseLayerElementSettings()
        {
//            Color = new SKColor(0, 0, 0);
//            BlendMode = SKBlendMode.Color;
//            XScale = 0.5f;
//            YScale = 0.5f;
//            AnimationSpeed = 50f;
        }

        public SKColor Color
        {
            get => _color;
            set => SetAndNotify(ref _color, value);
        }

        public SKBlendMode BlendMode
        {
            get => _blendMode;
            set => SetAndNotify(ref _blendMode, value);
        }

        public float XScale
        {
            get => _xScale;
            set => SetAndNotify(ref _xScale, value);
        }

        public float YScale
        {
            get => _yScale;
            set => SetAndNotify(ref _yScale, value);
        }

        public float AnimationSpeed
        {
            get => _animationSpeed;
            set => SetAndNotify(ref _animationSpeed, value);
        }
    }
}
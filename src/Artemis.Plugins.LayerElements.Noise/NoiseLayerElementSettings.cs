using Artemis.Core.Plugins.LayerElement;
using SkiaSharp;

namespace Artemis.Plugins.LayerElements.Noise
{
    public class NoiseLayerElementSettings : LayerElementSettings
    {
        private SKBlendMode _blendMode;
        private float _xScale;
        private float _yScale;
        private float _animationSpeed;


        public NoiseLayerElementSettings()
        {
            BlendMode = SKBlendMode.Color;
            XScale = 0.5f;
            YScale = 0.5f;
            AnimationSpeed = 50f;
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

        public SKBlendMode BlendMode
        {
            get => _blendMode;
            set => SetAndNotify(ref _blendMode, value);
        }
    }
}
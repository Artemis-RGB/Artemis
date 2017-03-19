using System;
using System.Windows.Media;
using Artemis.Profiles.Layers.Interfaces;
using Artemis.Profiles.Layers.Models;
using Betwixt;

namespace Artemis.Profiles.Layers.Animations
{
    public class PulseAnimation : ILayerAnimation
    {
        private bool _increase = true;

        private Tweener<float> _opacityTweener = new Tweener<float>(0, 1000, 1000, Ease.Quad.InOut, LerpFuncFloat);
        public string Name => "Pulse";

        public void Update(LayerModel layerModel, bool updateAnimations)
        {
            var animationSpeed = 3.1 - layerModel.Properties.AnimationSpeed;
            // TODO: Generic implementation
            // Reset animation progress if layer wasn't drawn for 100ms
            if (new TimeSpan(0, 0, 0, 0, 100) < DateTime.Now - layerModel.LastRender && updateAnimations || MustExpire(layerModel))
            {
                _opacityTweener = new Tweener<float>(0, 1000, animationSpeed * 1000, Ease.Quad.InOut, LerpFuncFloat);
                _increase = true;
            }

            // Update animation progress
            if (!updateAnimations)
                return;

            if (!_opacityTweener.Running && _increase)
            {
                _opacityTweener = new Tweener<float>(1000, 0, animationSpeed * 1000, Ease.Quad.InOut, LerpFuncFloat);
                _increase = false;
            }

            _opacityTweener.Update(40);

            if (_increase)
                layerModel.AnimationProgress = _opacityTweener.Value / 1000;
            else
                layerModel.AnimationProgress = 1 + (1 - _opacityTweener.Value / 1000);
        }

        public void Draw(LayerModel layerModel, DrawingContext c, int drawScale)
        {
            if (layerModel.Brush == null || _opacityTweener == null)
                return;

            // Set up variables for this frame
            var rect = layerModel.Properties.Contain
                ? layerModel.LayerRect(drawScale)
                : layerModel.Properties.PropertiesRect(drawScale);

            var clip = layerModel.LayerRect(drawScale);

            // Can't meddle with the original brush because it's frozen.
            var brush = layerModel.Brush.Clone();
            brush.Opacity = _opacityTweener.Value / 1000;
            layerModel.Brush = brush;

            c.PushClip(new RectangleGeometry(clip));
            c.DrawRectangle(layerModel.Brush, null, rect);
            c.Pop();
        }

        public bool MustExpire(LayerModel layer) => layer.AnimationProgress >= 2;

        private static float LerpFuncFloat(float start, float end, float percent)
        {
            return start + (end - start) * percent;
        }
    }
}

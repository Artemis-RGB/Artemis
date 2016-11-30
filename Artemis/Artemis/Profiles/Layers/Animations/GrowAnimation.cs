using System;
using System.Windows.Media;
using Artemis.Profiles.Layers.Interfaces;
using Artemis.Profiles.Layers.Models;

namespace Artemis.Profiles.Layers.Animations
{
    public class GrowAnimation : ILayerAnimation
    {
        public string Name => "Grow";

        public void Update(LayerModel layerModel, bool updateAnimations)
        {
            // TODO: Generic implementation
            // Reset animation progress if layer wasn't drawn for 100ms
            if ((new TimeSpan(0, 0, 0, 0, 100) < DateTime.Now - layerModel.LastRender) && updateAnimations)
                layerModel.AnimationProgress = 0;

            var progress = layerModel.AnimationProgress;

            if (MustExpire(layerModel))
                progress = 0;
            progress = progress + layerModel.Properties.AnimationSpeed/2.5;

            // If not previewing, store the animation progress in the actual model for the next frame
            if (updateAnimations)
                layerModel.AnimationProgress = progress;
        }

        public void Draw(LayerModel layerModel, DrawingContext c)
        {
            if (layerModel.Brush == null)
                return;

            // Set up variables for this frame
            var rect = layerModel.Properties.Contain
                ? layerModel.LayerRect()
                : layerModel.Properties.PropertiesRect();

            var clip = layerModel.LayerRect();

            // Take an offset of 4 to allow layers to slightly leave their bounds
            var progress = (6.0 - layerModel.AnimationProgress)*10.0;
            if (progress < 0)
            {
                // Can't meddle with the original brush because it's frozen.
                var brush = layerModel.Brush.Clone();
                brush.Opacity = 1 + 0.025*progress;
                if (brush.Opacity < 0)
                    brush.Opacity = 0;
                if (brush.Opacity > 1)
                    brush.Opacity = 1;
                layerModel.Brush = brush;
            }
            rect.Inflate(-rect.Width/100.0*progress, -rect.Height/100.0*progress);
            clip.Inflate(-clip.Width/100.0*progress, -clip.Height/100.0*progress);

            c.PushClip(new RectangleGeometry(clip));
            c.DrawRectangle(layerModel.Brush, null, rect);
            c.Pop();
        }

        public bool MustExpire(LayerModel layer) => layer.AnimationProgress > 10;
    }
}
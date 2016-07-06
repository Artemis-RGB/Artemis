﻿using System.Windows;
using System.Windows.Media;
using Artemis.Profiles.Layers.Interfaces;
using Artemis.Profiles.Layers.Models;

namespace Artemis.Profiles.Layers.Animations
{
    public class GrowAnimation : ILayerAnimation
    {
        public string Name { get; } = "Grow";

        public void Update(LayerModel layerModel, bool updateAnimations)
        {
            var progress = layerModel.Properties.AnimationProgress;

            if (MustExpire(layerModel))
                progress = 0;
            progress = progress + layerModel.Properties.AnimationSpeed/2.5;

            // If not previewing, store the animation progress in the actual model for the next frame
            if (updateAnimations)
                layerModel.Properties.AnimationProgress = progress;
        }

        public void Draw(LayerPropertiesModel props, LayerPropertiesModel applied, DrawingContext c)
        {
            if (applied.Brush == null)
                return;

            const int scale = 4;
            // Set up variables for this frame
            var rect = props.Contain
                ? new Rect(applied.X*scale, applied.Y*scale, applied.Width*scale, applied.Height*scale)
                : new Rect(props.X*scale, props.Y*scale, props.Width*scale, props.Height*scale);

            var clip = new Rect(applied.X*scale, applied.Y*scale, applied.Width*scale, applied.Height*scale);

            // Take an offset of 4 to allow layers to slightly leave their bounds
            var progress = (6.0 - props.AnimationProgress)*10.0;
            if (progress < 0)
            {
                // Can't meddle with the original brush because it's frozen.
                var brush = applied.Brush.Clone();
                brush.Opacity = 1 + 0.025*progress;
                if (brush.Opacity < 0)
                    brush.Opacity = 0;
                if (brush.Opacity > 1)
                    brush.Opacity = 1;
                applied.Brush = brush;
            }
            rect.Inflate(-rect.Width/100.0*progress, -rect.Height/100.0*progress);
            clip.Inflate(-clip.Width/100.0*progress, -clip.Height/100.0*progress);

            c.PushClip(new RectangleGeometry(clip));
            c.DrawRectangle(applied.Brush, null, rect);
            c.Pop();
        }

        public bool MustExpire(LayerModel layer) => layer.Properties.AnimationProgress > 10;
    }
}
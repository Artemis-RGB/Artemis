using System;
using Artemis.Models.Profiles.Properties;

namespace Artemis.Utilities.Layers
{
    public static class AnimationUpdater
    {
        public static void UpdateAnimation(KeyboardPropertiesModel properties)
        {
            const int scale = 4;

            // Horizontal sliding
            if (properties.Animation == LayerAnimation.SlideRight || properties.Animation == LayerAnimation.SlideLeft)
            {
                if (properties.AnimationProgress > properties.Width*scale)
                    properties.AnimationProgress = 0;
            }

            // Vertical sliding
            if (properties.Animation == LayerAnimation.SlideDown || properties.Animation == LayerAnimation.SlideUp)
            {
                if (properties.AnimationProgress > properties.Height*scale)
                    properties.AnimationProgress = 0;
            }

            // Pulse animation
            if (properties.Animation == LayerAnimation.Pulse)
            {
                var opac = (Math.Sin(properties.AnimationProgress*Math.PI) + 1)*(properties.Opacity/2);
                properties.Opacity = opac;
                if (properties.AnimationProgress > 2)
                    properties.AnimationProgress = 0;

                properties.AnimationProgress = (int) (properties.AnimationProgress + properties.AnimationSpeed/2);
            }
            else
            {
                // ApplyProperties the animation progress
                properties.AnimationProgress = (int) (properties.AnimationProgress + properties.AnimationSpeed);
            }
        }
    }
}
using System;
using Artemis.Models.Profiles.Properties;

namespace Artemis.Utilities.Layers
{
    public static class AnimationUpdater
    {
        public static void UpdateAnimation(KeyboardPropertiesModel properties)
        {
            const int scale = 4;
            var progress = properties.AnimationProgress;

            // Horizontal sliding
            if (properties.Animation == LayerAnimation.SlideRight || properties.Animation == LayerAnimation.SlideLeft)
            {
                if (progress > properties.Width*scale)
                    progress = 0;
            }

            // Vertical sliding
            if (properties.Animation == LayerAnimation.SlideDown || properties.Animation == LayerAnimation.SlideUp)
            {
                if (progress > properties.Height*scale)
                    progress = 0;
            }

            // Pulse animation
            if (properties.Animation == LayerAnimation.Pulse)
            {
                if (progress > 2)
                    progress = 0;

                progress = progress + properties.AnimationSpeed/2;
            }
            else
            {
                progress = progress + properties.AnimationSpeed * 2;
            }

            properties.AnimationProgress = progress;
        }
    }
}
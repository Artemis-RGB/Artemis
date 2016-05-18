using Artemis.Models.Profiles.Properties;

namespace Artemis.Utilities.Layers
{
    public static class AnimationUpdater
    {
        public static void UpdateAnimation(KeyboardPropertiesModel properties, KeyboardPropertiesModel appliedProperties)
        {
            const int scale = 4;
            var animateProperties = properties.Contain ? appliedProperties : properties;
            var progress = appliedProperties.AnimationProgress;

            // Horizontal sliding
            if (animateProperties.Animation == LayerAnimation.SlideRight ||
                animateProperties.Animation == LayerAnimation.SlideLeft)
            {
                if (progress > animateProperties.Width*scale)
                    progress = 0;
            }

            // Vertical sliding
            if (animateProperties.Animation == LayerAnimation.SlideDown ||
                animateProperties.Animation == LayerAnimation.SlideUp)
            {
                if (progress > animateProperties.Height*scale)
                    progress = 0;
            }

            // Pulse animation
            if (animateProperties.Animation == LayerAnimation.Pulse)
            {
                if (progress > 2)
                    progress = 0;

                progress = progress + animateProperties.AnimationSpeed/2;
            }
            else
            {
                progress = progress + animateProperties.AnimationSpeed*2;
            }

            appliedProperties.AnimationProgress = progress;
            // Store the animation progress in the actual model for the next frame
            properties.AnimationProgress = progress;
        }
    }
}
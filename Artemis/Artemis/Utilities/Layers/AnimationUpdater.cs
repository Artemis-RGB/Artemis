using Artemis.Models.Profiles.Properties;

namespace Artemis.Utilities.Layers
{
    public static class AnimationUpdater
    {
        public static void UpdateAnimation(KeyboardPropertiesModel properties, KeyboardPropertiesModel appliedProperties, bool updateAnimations)
        {
            const int scale = 4;
            var animateProperties = properties.Contain ? appliedProperties : properties;
            appliedProperties.AnimationProgress = properties.AnimationProgress;
            var progress = appliedProperties.AnimationProgress;

            switch (animateProperties.Animation)
            {
                case LayerAnimation.SlideRight:
                case LayerAnimation.SlideLeft:
                    if (progress + 1 >= animateProperties.Width*scale)
                        progress = 0;
                    progress = progress + animateProperties.AnimationSpeed * 2;
                    break;
                case LayerAnimation.SlideDown:
                case LayerAnimation.SlideUp:
                    if (progress + 1 >= animateProperties.Height*scale)
                        progress = 0;
                    progress = progress + animateProperties.AnimationSpeed * 2;
                    break;
                case LayerAnimation.Pulse:
                    if (progress > 2)
                        progress = 0;
                    progress = progress + animateProperties.AnimationSpeed/2;
                    break;
                case LayerAnimation.Grow:
                    if (progress > 10)
                        progress = 0;
                    progress = progress + animateProperties.AnimationSpeed / 2.5;
                    break;
                default:
                    progress = progress + animateProperties.AnimationSpeed*2;
                    break;
            }

            appliedProperties.AnimationProgress = progress;

            // If not previewing, store the animation progress in the actual model for the next frame
            if (updateAnimations)
                properties.AnimationProgress = progress;
        }
    }
}
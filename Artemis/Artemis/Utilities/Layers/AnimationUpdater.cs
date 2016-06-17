using Artemis.Models.Profiles.Layers;

namespace Artemis.Utilities.Layers
{
    public static class AnimationUpdater
    {
        public static void UpdateAnimation(KeyboardPropertiesModel properties, bool updateAnimations)
        {
            var progress = properties.AnimationProgress;

            switch (properties.Animation)
            {
                case LayerAnimation.SlideRight:
                case LayerAnimation.SlideLeft:
                    if (progress + properties.AnimationSpeed * 2 >= properties.Width*4)
                        progress = 0;
                    progress = progress + properties.AnimationSpeed*2;
                    break;
                case LayerAnimation.SlideDown:
                case LayerAnimation.SlideUp:
                    if (progress + properties.AnimationSpeed * 2 >= properties.Height*4)
                        progress = 0;
                    progress = progress + properties.AnimationSpeed*2;
                    break;
                case LayerAnimation.Pulse:
                    if (progress > 2)
                        progress = 0;
                    progress = progress + properties.AnimationSpeed/2;
                    break;
                case LayerAnimation.Grow:
                    if (progress > 10)
                        progress = 0;
                    progress = progress + properties.AnimationSpeed/2.5;
                    break;
                default:
                    progress = progress + properties.AnimationSpeed*2;
                    break;
            }

            // If not previewing, store the animation progress in the actual model for the next frame
            if (updateAnimations)
                properties.AnimationProgress = progress;
        }
    }
}
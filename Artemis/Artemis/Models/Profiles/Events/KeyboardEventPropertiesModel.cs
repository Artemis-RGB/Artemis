using System;
using Artemis.Models.Profiles.Layers;

namespace Artemis.Models.Profiles.Events
{
    public class KeyboardEventPropertiesModel : EventPropertiesModel
    {
        public override void TriggerEvent(LayerModel layer)
        {
            var keyboardProperties = layer.Properties as KeyboardPropertiesModel;
            if (keyboardProperties == null)
                throw new ArgumentException("Layer's properties cannot be null " +
                                            "and must be of type KeyboardPropertiesModel");
            if (!MustTrigger)
                return;

            MustTrigger = false;
            keyboardProperties.AnimationProgress = 0.0;
            layer.GifImage.CurrentFrame = 0;
        }

        public override bool MustStop(LayerModel layer)
        {
            var keyboardProperties = layer.Properties as KeyboardPropertiesModel;
            if (keyboardProperties == null)
                throw new ArgumentException("Layer's properties cannot be null " +
                                            "and must be of type KeyboardPropertiesModel");

            switch (ExpirationType)
            {
                case ExpirationType.Time:
                    return DateTime.Now - AnimationStart > Length;
                case ExpirationType.Animation:
                    if (keyboardProperties.Animation == LayerAnimation.None)
                        return true;
                    if (layer.LayerType == LayerType.KeyboardGif)
                        return layer.GifImage.FrameCount >= layer.GifImage.CurrentFrame;
                    if (keyboardProperties.Animation == LayerAnimation.Grow)
                        return keyboardProperties.AnimationProgress > 10;
                    if (keyboardProperties.Animation == LayerAnimation.Pulse)
                        return keyboardProperties.AnimationProgress > 2;

                    return keyboardProperties.AnimationProgress + keyboardProperties.AnimationSpeed*2 >=
                           keyboardProperties.Height*4;
                default:
                    return true;
            }
        }
    }
}
using System;
using Artemis.Profiles.Layers.Types.Keyboard;
using Artemis.Profiles.Layers.Types.KeyboardGif;

namespace Artemis.Profiles.Layers.Models
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
            MustDraw = true;
            keyboardProperties.AnimationProgress = 0.0;
            if (layer.GifImage != null)
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
                    if (AnimationStart == DateTime.MinValue)
                        return false;
                    return DateTime.Now - Length > AnimationStart;
                case ExpirationType.Animation:
                    if (layer.LayerType is KeyboardGifType)
                        return layer.GifImage?.CurrentFrame >= layer.GifImage?.FrameCount - 1;
                    return layer.LayerAnimation.MustExpire(layer);
                default:
                    return true;
            }
        }
    }
}
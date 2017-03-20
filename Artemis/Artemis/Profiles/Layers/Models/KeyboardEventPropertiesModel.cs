using System;
using Artemis.Profiles.Layers.Types.KeyboardGif;

namespace Artemis.Profiles.Layers.Models
{
    public class KeyboardEventPropertiesModel : EventPropertiesModel
    {
        public override void TriggerEvent(LayerModel layer)
        {
            if (CanTrigger)
            {
                if (layer.GifImage != null)
                    layer.GifImage.CurrentFrame = 0;
            }

            base.TriggerEvent(layer);
        }

        public override bool MustStop(LayerModel layer)
        {
            if (layer.LayerType is KeyboardGifType && ExpirationType == ExpirationType.Animation)
                return layer.GifImage?.CurrentFrame >= layer.GifImage?.FrameCount - 1;

            return base.MustStop(layer);
        }
    }
}

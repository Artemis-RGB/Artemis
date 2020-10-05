using System;

namespace Artemis.Core.LayerEffects.Placeholder
{
    internal static class PlaceholderLayerEffectDescriptor
    {
        public static LayerEffectDescriptor Create(Guid missingPluginGuid)
        {
            LayerEffectDescriptor descriptor = new LayerEffectDescriptor("Missing effect", "This effect could not be loaded", "FileQuestion", null, Constants.EffectPlaceholderPlugin)
            {
                PlaceholderFor = missingPluginGuid
            };

            return descriptor;
        }
    }
}
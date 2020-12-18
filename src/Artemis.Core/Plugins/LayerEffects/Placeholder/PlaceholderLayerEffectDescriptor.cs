namespace Artemis.Core.LayerEffects.Placeholder
{
    internal static class PlaceholderLayerEffectDescriptor
    {
        public static LayerEffectDescriptor Create(string missingProviderId)
        {
            LayerEffectDescriptor descriptor = new("Missing effect", "This effect could not be loaded", "FileQuestion", null, Constants.EffectPlaceholderPlugin)
            {
                PlaceholderFor = missingProviderId
            };

            return descriptor;
        }
    }
}
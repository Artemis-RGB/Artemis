namespace Artemis.Core.LayerEffects.Placeholder
{
    internal static class PlaceholderLayerEffectDescriptor
    {
        public static LayerEffectDescriptor Create()
        {
            var descriptor = new LayerEffectDescriptor("Missing effect", "This effect could not be loaded", "FileQuestion", null, null) {IsPlaceHolder = true};
            return descriptor;
        }
    }
}
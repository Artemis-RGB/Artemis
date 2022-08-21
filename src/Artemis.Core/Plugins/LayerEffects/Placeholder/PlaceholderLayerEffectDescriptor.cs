namespace Artemis.Core.LayerEffects.Placeholder;

internal static class PlaceholderLayerEffectDescriptor
{
    public static LayerEffectDescriptor Create(string missingProviderId)
    {
        LayerEffectDescriptor descriptor = new(missingProviderId, Constants.EffectPlaceholderPlugin);
        return descriptor;
    }
}
namespace Artemis.Core.LayerEffects.Placeholder;

internal static class PlaceholderLayerEffectDescriptor
{
    public static LayerEffectDescriptor Create()
    {
        LayerEffectDescriptor descriptor = LayerEffectDescriptor.CreatePlaceholder(Constants.EffectPlaceholderPlugin);
        return descriptor;
    }
}
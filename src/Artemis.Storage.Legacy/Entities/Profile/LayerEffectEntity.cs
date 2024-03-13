namespace Artemis.Storage.Legacy.Entities.Profile;

internal class LayerEffectEntity
{
    public string ProviderId { get; set; } = string.Empty;
    public string EffectType { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool HasBeenRenamed { get; set; }
    public int Order { get; set; }

    public PropertyGroupEntity? PropertyGroup { get; set; }
}
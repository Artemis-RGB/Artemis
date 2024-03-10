namespace Artemis.Storage.Legacy.Entities.Profile;

internal class LayerBrushEntity
{
    public string ProviderId { get; set; } = string.Empty;
    public string BrushType { get; set; } = string.Empty;

    public PropertyGroupEntity? PropertyGroup { get; set; }
}
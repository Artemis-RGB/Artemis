namespace Artemis.Storage.Migrator.Legacy.Entities.Profile;

public class LayerBrushEntity
{
    public string ProviderId { get; set; } = string.Empty;
    public string BrushType { get; set; } = string.Empty;

    public PropertyGroupEntity? PropertyGroup { get; set; }
}
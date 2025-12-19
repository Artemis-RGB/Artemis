namespace Artemis.Storage.Legacy.Entities.Profile;

internal class PropertyGroupEntity
{
    public string Identifier { get; set; } = string.Empty;
    public List<PropertyEntity> Properties { get; set; } = [];
    public List<PropertyGroupEntity> PropertyGroups { get; set; } = [];
}
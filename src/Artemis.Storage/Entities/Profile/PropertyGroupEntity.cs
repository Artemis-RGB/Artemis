using System.Collections.Generic;

namespace Artemis.Storage.Entities.Profile;

public class PropertyGroupEntity
{
    public string Identifier { get; set; }
    public List<PropertyEntity> Properties { get; set; } = new();
    public List<PropertyGroupEntity> PropertyGroups { get; set; } = new();
}
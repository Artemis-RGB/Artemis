using System.Collections.Generic;

namespace Artemis.Storage.Entities.Profile;

public class PropertyGroupEntity
{
    public string Identifier { get; set; } = string.Empty;
    public List<PropertyEntity> Properties { get; set; } = [];
    public List<PropertyGroupEntity> PropertyGroups { get; set; } = [];
}
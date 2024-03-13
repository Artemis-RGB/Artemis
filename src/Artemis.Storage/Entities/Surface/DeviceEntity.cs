using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Artemis.Storage.Entities.Surface;

public class DeviceEntity
{
    public DeviceEntity()
    {
        InputIdentifiers = new List<DeviceInputIdentifierEntity>();
        InputMappings = new List<InputMappingEntity>();
        Categories = new List<int>();
    }

    [MaxLength(512)]
    public string Id { get; set; } = string.Empty;

    [MaxLength(512)]
    public string DeviceProvider { get; set; } = string.Empty;

    public float X { get; set; }
    public float Y { get; set; }
    public float Rotation { get; set; }
    public float Scale { get; set; }
    public int ZIndex { get; set; }
    public float RedScale { get; set; }
    public float GreenScale { get; set; }
    public float BlueScale { get; set; }
    public bool IsEnabled { get; set; }

    public int PhysicalLayout { get; set; }

    [MaxLength(32)]
    public string? LogicalLayout { get; set; }

    [MaxLength(64)]
    public string? LayoutType { get; set; }

    [MaxLength(512)]
    public string? LayoutParameter { get; set; }

    public List<DeviceInputIdentifierEntity> InputIdentifiers { get; set; }
    public List<InputMappingEntity> InputMappings { get; set; }
    public List<int> Categories { get; set; }
}

public class InputMappingEntity
{
    public int OriginalLedId { get; set; }
    public int MappedLedId { get; set; }
}

public class DeviceInputIdentifierEntity
{
    public string InputProvider { get; set; } = string.Empty;
    public string Identifier { get; set; } = string.Empty;
}
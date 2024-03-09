namespace Artemis.Storage.Migrator.Legacy.Entities.Surface;

public class DeviceEntity
{
    public DeviceEntity()
    {
        InputIdentifiers = new List<DeviceInputIdentifierEntity>();
        InputMappings = new List<InputMappingEntity>();
        Categories = new List<int>();
    }

    public string Id { get; set; } = string.Empty;
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
    public string? LogicalLayout { get; set; }
    public string? LayoutType { get; set; }
    public string? LayoutParameter { get; set; }

    public List<DeviceInputIdentifierEntity> InputIdentifiers { get; set; }
    public List<InputMappingEntity> InputMappings { get; set; }
    public List<int> Categories { get; set; }

    public Storage.Entities.Surface.DeviceEntity Migrate()
    {
        // All properties match, return a copy
        return new Storage.Entities.Surface.DeviceEntity()
        {
            Id = Id,
            DeviceProvider = DeviceProvider,
            X = X,
            Y = Y,
            Rotation = Rotation,
            Scale = Scale,
            ZIndex = ZIndex,
            RedScale = RedScale,
            GreenScale = GreenScale,
            BlueScale = BlueScale,
            IsEnabled = IsEnabled,
            PhysicalLayout = PhysicalLayout,
            LogicalLayout = LogicalLayout,
            LayoutType = LayoutType,
            LayoutParameter = LayoutParameter,
            InputIdentifiers = InputIdentifiers.Select(i => new Storage.Entities.Surface.DeviceInputIdentifierEntity
            {
                InputProvider = i.InputProvider,
                Identifier = i.Identifier.ToString() ?? string.Empty
            }).ToList(),
            InputMappings = InputMappings.Select(i => new Storage.Entities.Surface.InputMappingEntity
            {
                OriginalLedId = i.OriginalLedId,
                MappedLedId = i.MappedLedId
            }).ToList(),
            Categories = Categories
        };
    }
}

public class InputMappingEntity
{
    public int OriginalLedId { get; set; }
    public int MappedLedId { get; set; }
}

public class DeviceInputIdentifierEntity
{
    public string InputProvider { get; set; } = string.Empty;
    public object Identifier { get; set; } = string.Empty;
}
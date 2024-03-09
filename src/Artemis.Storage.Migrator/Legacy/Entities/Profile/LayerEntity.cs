using Artemis.Storage.Migrator.Legacy.Entities.Profile.Abstract;
using Artemis.Storage.Migrator.Legacy.Entities.Profile.AdaptionHints;
using LiteDB;

namespace Artemis.Storage.Migrator.Legacy.Entities.Profile;

public class LayerEntity : RenderElementEntity
{
    public LayerEntity()
    {
        Leds = new List<LedEntity>();
        AdaptionHints = new List<IAdaptionHintEntity>();
    }

    public int Order { get; set; }
    public string? Name { get; set; }
    public bool Suspended { get; set; }

    public List<LedEntity> Leds { get; set; }
    public List<IAdaptionHintEntity> AdaptionHints { get; set; }

    public PropertyGroupEntity? GeneralPropertyGroup { get; set; }
    public PropertyGroupEntity? TransformPropertyGroup { get; set; }
    public LayerBrushEntity? LayerBrush { get; set; }

    [BsonRef("ProfileEntity")]
    public ProfileEntity Profile { get; set; } = null!;

    public Guid ProfileId { get; set; }
}
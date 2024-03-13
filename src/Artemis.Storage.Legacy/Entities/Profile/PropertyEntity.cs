using Artemis.Storage.Legacy.Entities.Profile.DataBindings;

namespace Artemis.Storage.Legacy.Entities.Profile;

internal class PropertyEntity
{
    public string Identifier { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public bool KeyframesEnabled { get; set; }

    public DataBindingEntity? DataBinding { get; set; }
    public List<KeyframeEntity> KeyframeEntities { get; set; } = new();
}
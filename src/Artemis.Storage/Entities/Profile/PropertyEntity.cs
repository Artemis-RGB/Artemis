using System.Collections.Generic;
using Artemis.Storage.Entities.Profile.DataBindings;

namespace Artemis.Storage.Entities.Profile;

public class PropertyEntity
{
    public string Identifier { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public bool KeyframesEnabled { get; set; }

    public DataBindingEntity? DataBinding { get; set; }
    public List<KeyframeEntity> KeyframeEntities { get; set; } = [];
}
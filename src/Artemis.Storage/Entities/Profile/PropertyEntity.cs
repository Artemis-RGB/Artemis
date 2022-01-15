using System.Collections.Generic;
using Artemis.Storage.Entities.Profile.DataBindings;

namespace Artemis.Storage.Entities.Profile;

public class PropertyEntity
{
    public string Identifier { get; set; }
    public string Value { get; set; }
    public bool KeyframesEnabled { get; set; }

    public DataBindingEntity DataBinding { get; set; }
    public List<KeyframeEntity> KeyframeEntities { get; set; } = new();
}
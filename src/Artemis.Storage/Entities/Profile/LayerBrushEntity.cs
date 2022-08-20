using System.Collections.Generic;

namespace Artemis.Storage.Entities.Profile;

public class LayerBrushEntity
{
    public string ProviderId { get; set; }
    public string BrushType { get; set; }

    public PropertyGroupEntity PropertyGroup { get; set; }
}
using System.Collections.Generic;
using Artemis.Core.Models.Profile.LayerProperties;
using Stylet;

namespace Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.Abstract
{
    public abstract class LayerPropertyBaseViewModel : PropertyChangedBase
    {
        public abstract List<BaseLayerPropertyKeyframe> GetKeyframes(bool visibleOnly);
    }
}
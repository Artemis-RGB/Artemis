using System.Collections.ObjectModel;
using Artemis.Core;
using Artemis.UI.Shared;

namespace Artemis.UI.Screens.ProfileEditor.Properties;

public abstract class PropertyViewModelBase : ViewModelBase
{
    public abstract ReadOnlyObservableCollection<ILayerPropertyKeyframe> Keyframes { get; }
}
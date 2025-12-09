using Artemis.Core;
using ReactiveUI;

namespace Artemis.UI.Screens.Profile.ProfileEditor.Properties.Tree;

public interface ITreePropertyViewModel : IReactiveObject
{
    ILayerProperty BaseLayerProperty { get; }
    bool DataBindingEnabled { get; }
    double GetDepth();
    void ToggleCurrentLayerProperty();
}
using Artemis.Core;
using ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor.ProfileElementProperties.Tree;

public interface ITreePropertyViewModel : IReactiveObject
{
    ILayerProperty BaseLayerProperty { get; }
    bool HasDataBinding { get; }
    double GetDepth();
}
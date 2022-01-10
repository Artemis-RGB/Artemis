using ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor.ProfileElementProperties.Tree;

public interface ITreePropertyViewModel : IReactiveObject
{
    bool HasDataBinding { get; }
    double GetDepth();
}
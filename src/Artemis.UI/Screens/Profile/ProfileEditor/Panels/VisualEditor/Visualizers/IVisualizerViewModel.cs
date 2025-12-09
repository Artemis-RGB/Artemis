using Artemis.Core;

namespace Artemis.UI.Screens.Profile.ProfileEditor.VisualEditor.Visualizers;

public interface IVisualizerViewModel
{
    ProfileElement? ProfileElement { get; }
    double X { get; }
    double Y { get; }
    int Order { get; }
}
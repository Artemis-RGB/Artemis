using Artemis.Core;

namespace Artemis.UI.Screens.ProfileEditor.VisualEditor.Visualizers;

public interface IVisualizerViewModel
{
    ProfileElement? ProfileElement { get; }
    double X { get; }
    double Y { get; }
    int Order { get; }
}
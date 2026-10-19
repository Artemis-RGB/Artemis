using SkiaSharp;

namespace Artemis.Core.Tests.Infrastructure.VisualScripting;

/// <summary>
///     A node that outputs a color the test controls. The color is stored, so it survives saving and loading a script
///     when the node is created through <see cref="Plugins.Nodes.DataModelTestNodeProvider" />.
/// </summary>
[Node("Test color", "Outputs a color controlled by the test", "Tests", OutputType = typeof(SKColor))]
public class TestColorNode : Node<SKColor>
{
    public TestColorNode()
    {
        Output = CreateOutputPin<SKColor>();
    }

    public OutputPin<SKColor> Output { get; }

    /// <summary>
    ///     Gets or sets the color the node outputs
    /// </summary>
    public SKColor Value
    {
        get => Storage;
        set => Storage = value;
    }

    public override void Evaluate()
    {
        Output.Value = Value;
    }
}

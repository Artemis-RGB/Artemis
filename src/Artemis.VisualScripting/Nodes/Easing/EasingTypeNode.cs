using Artemis.Core;
using Artemis.VisualScripting.Nodes.Easing.Screens;

namespace Artemis.VisualScripting.Nodes.Easing;

[Node("Easing Type", "Outputs a selectable easing type.", "Easing", OutputType = typeof(Easings.Functions))]
public class EasingTypeNode : Node<Easings.Functions, EasingTypeNodeCustomViewModel>
{
    public EasingTypeNode()
    {
        Output = CreateOutputPin<Easings.Functions>();
    }

    public OutputPin<Easings.Functions> Output { get; }

    public override void Evaluate()
    {
        Output.Value = Storage;
    }
}
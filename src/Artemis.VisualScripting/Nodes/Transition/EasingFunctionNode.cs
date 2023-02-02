using Artemis.Core;
using Artemis.VisualScripting.Nodes.Transition.Screens;

namespace Artemis.VisualScripting.Nodes.Transition;

[Node("Easing Function", "Outputs a selectable easing function", "Transition", OutputType = typeof(Easings.Functions))]
public class EasingFunctionNode : Node<Easings.Functions, EasingFunctionNodeCustomViewModel>
{
    public EasingFunctionNode()
    {
        Output = CreateOutputPin<Easings.Functions>();
    }

    public OutputPin<Easings.Functions> Output { get; }

    public override void Evaluate()
    {
        Output.Value = Storage;
    }
}
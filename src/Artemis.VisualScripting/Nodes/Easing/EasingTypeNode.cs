using Artemis.Core;
using Artemis.VisualScripting.Nodes.Easing.CustomViewModels;

namespace Artemis.VisualScripting.Nodes.Easing;

[Node("Easing Type", "Outputs a selectable easing type.", "Easing", OutputType = typeof(Easings.Functions))]
public class EasingTypeNode : Node<Easings.Functions, EasingTypeNodeCustomViewModel>
{
    public EasingTypeNode() : base("Easing Type", "Outputs a selectable easing type.")
    {
        Output = CreateOutputPin<Easings.Functions>();
    }

    public OutputPin<Easings.Functions> Output { get; }

    public override void Evaluate()
    {
        Output.Value = Storage;
    }
}
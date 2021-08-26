using Artemis.Core;
using Artemis.VisualScripting.Nodes.Easing.CustomViewModels;

namespace Artemis.VisualScripting.Nodes.Easing
{
    [Node("Easing Type", "Outputs a selectable easing type.", "Easing", OutputType = typeof(Easings.Functions))]
    public class EasingTypeNode : Node<EasingTypeNodeCustomViewModel>
    {
        public EasingTypeNode() : base("Easing Type", "Outputs a selectable easing type.")
        {
            Output = CreateOutputPin<Easings.Functions>();
        }

        public OutputPin<Easings.Functions> Output { get; }
        public Easings.Functions EasingFunction => Storage as Easings.Functions? ?? Easings.Functions.Linear;

        public override void Evaluate()
        {
            Output.Value = EasingFunction;
        }
    }
}
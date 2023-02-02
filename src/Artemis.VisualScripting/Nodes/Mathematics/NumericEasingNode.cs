using Artemis.Core;

namespace Artemis.VisualScripting.Nodes.Mathematics;

[Node("Numeric Easing", "Interpolates a value from 0-1 to 0-1 with the given function", "Mathematics", InputType = typeof(Numeric), OutputType = typeof(Numeric))]
public class NumericEasingNode : Node
{
    public InputPin<Numeric> Input { get; }
    
    public InputPin<Easings.Functions> EasingFunction { get; }

    public OutputPin<Numeric> Result { get; }

    public NumericEasingNode()
    {
        Input = CreateInputPin<Numeric>("Input");
        EasingFunction = CreateInputPin<Easings.Functions>("Function");

        Result = CreateOutputPin<Numeric>();
    }

    public override void Evaluate()
    {
        double inputValue = Input.Value;
        double progress = Math.Clamp(inputValue, 0, 1);
        
        Result.Value = Easings.Interpolate(progress, EasingFunction.Value);
    }
}
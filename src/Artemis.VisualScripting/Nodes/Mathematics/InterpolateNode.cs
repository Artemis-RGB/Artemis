using Artemis.Core;

namespace Artemis.VisualScripting.Nodes.Mathematics;

[Node("Interpolate", "Interpolates the value between the range, outputting number between 0-1",
    "Mathematics", InputType = typeof(Numeric), OutputType = typeof(Numeric))]
public class InterpolateNode : Node
{
    public InputPin<Numeric> Input { get; }
    public InputPin<Numeric> Start { get; }
    public InputPin<Numeric> End { get; }
    
    public InputPin<Easings.Functions> EasingFunction { get; }

    public OutputPin<Numeric> Result { get; }

    public InterpolateNode()
    {
        Input = CreateInputPin<Numeric>("Input");
        Start = CreateInputPin<Numeric>("Start");
        End = CreateInputPin<Numeric>("End");
        
        EasingFunction = CreateInputPin<Easings.Functions>("EasingFunction");

        Result = CreateOutputPin<Numeric>();
    }

    public override void Evaluate()
    {
        double inputValue = Input.Value;
        double startValue = Start.Value;
        double endValue = End.Value;
        double progress = (Math.Clamp(inputValue, startValue, endValue) - startValue) / endValue;
        
        Result.Value = Easings.Interpolate(progress, EasingFunction.Value);
    }
}
using Artemis.Core;

namespace Artemis.VisualScripting.Nodes.Mathematics;

[Node("Normalize", "Normalizes the number into range between 0-1", "Mathematics", InputType = typeof(Numeric), OutputType = typeof(Numeric))]
public class NormalizeNode : Node
{
    public InputPin<Numeric> Input { get; }
    public InputPin<Numeric> Start { get; }
    public InputPin<Numeric> End { get; }

    public OutputPin<Numeric> Result { get; }

    public NormalizeNode()
    {
        Input = CreateInputPin<Numeric>("Input");
        Start = CreateInputPin<Numeric>("Start");
        End = CreateInputPin<Numeric>("End");

        Result = CreateOutputPin<Numeric>();
    }

    public override void Evaluate()
    {
        double inputValue = Input.Value;
        double startValue = Start.Value;
        double endValue = End.Value;
        Result.Value = (Math.Clamp(inputValue, startValue, endValue) - startValue) / (endValue - startValue);
    }
}
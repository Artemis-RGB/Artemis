namespace Artemis.Core.Tests.Infrastructure.VisualScripting;

/// <summary>
///     A node that adds up the numbers on its inputs. The inputs are a pin collection that starts with two pins.
/// </summary>
[Node("Test sum", "Adds up its inputs", "Tests", InputType = typeof(Numeric), OutputType = typeof(Numeric))]
public class TestSumNode : Node
{
    public TestSumNode()
    {
        Values = CreateInputPinCollection<Numeric>("Values", 2);
        Result = CreateOutputPin<Numeric>("Result");
    }

    public InputPinCollection<Numeric> Values { get; }
    public OutputPin<Numeric> Result { get; }

    /// <summary>
    ///     Gets the number of times the node was evaluated
    /// </summary>
    public int Evaluations { get; private set; }

    public override void Evaluate()
    {
        Evaluations++;
        Result.Value = Values.Values.Sum();
    }
}

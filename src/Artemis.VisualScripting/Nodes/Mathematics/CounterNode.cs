using Artemis.Core;

namespace Artemis.VisualScripting.Nodes.Mathematics;

[Node("Counter", "Counts from 0.0 to 1.0 at a configurable rate.", "Mathematics", InputType = typeof(Numeric), OutputType = typeof(Numeric))]
public class CounterNode : Node
{
    private DateTime _lastEvaluate = DateTime.MinValue;
    private float _progress;

    public CounterNode()
        : base("Counter", "Counts from 0.0 to 1.0 at a configurable rate.")
    {
        Time = CreateInputPin<Numeric>("Time (ms)");
        Output = CreateOutputPin<Numeric>();
    }

    public InputPin<Numeric> Time { get; set; }
    public OutputPin<Numeric> Output { get; set; }

    public override void Evaluate()
    {
        DateTime now = DateTime.Now;
        TimeSpan delta = now - _lastEvaluate;

        if (Time.Value != 0)
            _progress = (float) (_progress + delta.TotalMilliseconds / Time.Value) % 1.0f;

        Output.Value = new Numeric(MathF.Round(_progress, 4, MidpointRounding.AwayFromZero));
        _lastEvaluate = now;
    }
}
using Artemis.Core;

namespace Artemis.VisualScripting.Nodes.Easing;

[Node("Numeric Easing", "Outputs an eased numeric value", "Easing", InputType = typeof(Numeric), OutputType = typeof(Numeric))]
public class NumericEasingNode : Node
{
    private float _currentValue;
    private DateTime _lastEvaluate = DateTime.MinValue;
    private float _progress;
    private float _sourceValue;
    private float _targetValue;

    public NumericEasingNode() : base("Numeric Easing", "Outputs an eased numeric value")
    {
        Input = CreateInputPin<Numeric>();
        EasingTime = CreateInputPin<Numeric>("delay");
        EasingFunction = CreateInputPin<Easings.Functions>("function");

        Output = CreateOutputPin<Numeric>();
    }

    public InputPin<Numeric> Input { get; set; }
    public InputPin<Numeric> EasingTime { get; set; }
    public InputPin<Easings.Functions> EasingFunction { get; set; }

    public OutputPin<Numeric> Output { get; set; }

    public override void Evaluate()
    {
        DateTime now = DateTime.Now;
        float inputValue = Input.Value;

        // If the value changed reset progress
        if (Math.Abs(_targetValue - inputValue) > 0.001f)
        {
            _sourceValue = _currentValue;
            _targetValue = Input.Value;
            _progress = 0f;
        }

        // Update until finished
        if (_progress < 1f)
        {
            Update();
            Output.Value = new Numeric(_currentValue);
        }
        // Stop updating past 1 and use the target value
        else
        {
            Output.Value = new Numeric(_targetValue);
        }

        _lastEvaluate = now;
    }

    private void Update()
    {
        float easingTime = EasingTime.Value != 0f ? EasingTime.Value : 1f;
        TimeSpan delta = DateTime.Now - _lastEvaluate;

        // In case of odd delta's, keep progress between 0f and 1f
        _progress = Math.Clamp(_progress + (float) delta.TotalMilliseconds / easingTime, 0f, 1f);

        double eased = _sourceValue + (_targetValue - _sourceValue) * Easings.Interpolate(_progress, EasingFunction.Value);
        _currentValue = (float) eased;
    }
}
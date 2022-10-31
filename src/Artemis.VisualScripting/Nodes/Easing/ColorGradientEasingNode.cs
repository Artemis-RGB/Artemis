using Artemis.Core;

namespace Artemis.VisualScripting.Nodes.Easing;

[Node("Color Gradient Easing", "Outputs an eased color gradient value", "Easing", InputType = typeof(ColorGradient), OutputType = typeof(ColorGradient))]
public class ColorGradientEasingNode : Node
{
    private DateTime _lastEvaluate = DateTime.MinValue;
    private float _progress;
    private ColorGradient? _currentValue;
    private ColorGradient? _sourceValue;
    private ColorGradient? _targetValue;

    public ColorGradientEasingNode()
    {
        Input = CreateInputPin<ColorGradient>();
        EasingTime = CreateInputPin<Numeric>("delay");
        EasingFunction = CreateInputPin<Easings.Functions>("function");

        Output = CreateOutputPin<ColorGradient>();
    }

    public InputPin<ColorGradient> Input { get; set; }
    public InputPin<Numeric> EasingTime { get; set; }
    public InputPin<Easings.Functions> EasingFunction { get; set; }

    public OutputPin<ColorGradient> Output { get; set; }

    public override void Evaluate()
    {
        DateTime now = DateTime.Now;
        
        if (Input.Value == null)
            return;

        // If the value changed reset progress
        if (!Equals(_targetValue, Input.Value))
        {
            _sourceValue = _currentValue ?? new ColorGradient(Input.Value);
            _targetValue = new ColorGradient(Input.Value);
            _progress = 0f;
        }

        // Update until finished
        if (_progress < 1f)
        {
            Update();
            Output.Value = _currentValue;
        }
        // Stop updating past 1 and use the target value
        else
        {
            Output.Value = _targetValue;
        }

        _lastEvaluate = now;
    }

    private void Update()
    {
        if (_sourceValue == null || _targetValue == null)
            return;
        
        float easingTime = EasingTime.Value != 0f ? EasingTime.Value : 1f;
        TimeSpan delta = DateTime.Now - _lastEvaluate;

        // In case of odd delta's, keep progress between 0f and 1f
        _progress = Math.Clamp(_progress + (float) delta.TotalMilliseconds / easingTime, 0f, 1f);
        _currentValue = _sourceValue.Interpolate(_targetValue, (float) Easings.Interpolate(_progress, EasingFunction.Value));
    }
}
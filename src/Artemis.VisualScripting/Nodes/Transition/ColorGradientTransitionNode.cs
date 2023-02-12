using Artemis.Core;

namespace Artemis.VisualScripting.Nodes.Transition;

[Node("Color Gradient Transition", "Outputs smoothly transitioned changes to the input color gradient", "Transition", InputType = typeof(ColorGradient), OutputType = typeof(ColorGradient))]
public class ColorGradientTransitionNode : Node
{
    private DateTime _lastEvaluate = DateTime.MinValue;
    private float _progress;
    private ColorGradient? _currentValue;
    private ColorGradient? _sourceValue;
    private ColorGradient? _targetValue;

    public ColorGradientTransitionNode()
    {
        Input = CreateInputPin<ColorGradient>();
        EasingTime = CreateInputPin<Numeric>("Delay");
        EasingFunction = CreateInputPin<Easings.Functions>("Function");

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
using Artemis.Core;
using SkiaSharp;

namespace Artemis.VisualScripting.Nodes.Easing;

[Node("Color Easing", "Outputs an eased color value", "Easing", InputType = typeof(SKColor), OutputType = typeof(SKColor))]
public class SKColorEasingNode : Node
{
    private SKColor _currentValue;
    private DateTime _lastEvaluate = DateTime.MinValue;
    private float _progress;
    private SKColor _sourceValue;
    private SKColor _targetValue;

    public SKColorEasingNode()
    {
        Input = CreateInputPin<SKColor>();
        EasingTime = CreateInputPin<Numeric>("delay");
        EasingFunction = CreateInputPin<Easings.Functions>("function");

        Output = CreateOutputPin<SKColor>();
    }

    public InputPin<SKColor> Input { get; set; }
    public InputPin<Numeric> EasingTime { get; set; }
    public InputPin<Easings.Functions> EasingFunction { get; set; }

    public OutputPin<SKColor> Output { get; set; }

    public override void Evaluate()
    {
        DateTime now = DateTime.Now;

        // If the value changed reset progress
        if (_targetValue != Input.Value)
        {
            _sourceValue = _currentValue;
            _targetValue = Input.Value;
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
        float easingTime = EasingTime.Value != 0f ? EasingTime.Value : 1f;
        TimeSpan delta = DateTime.Now - _lastEvaluate;

        // In case of odd delta's, keep progress between 0f and 1f
        _progress = Math.Clamp(_progress + (float) delta.TotalMilliseconds / easingTime, 0f, 1f);
        _currentValue = _sourceValue.Interpolate(_targetValue, (float) Easings.Interpolate(_progress, EasingFunction.Value));
    }
}
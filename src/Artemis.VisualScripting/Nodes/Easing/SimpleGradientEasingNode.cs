using Artemis.Core;
using SkiaSharp;

namespace Artemis.VisualScripting.Nodes.Easing;

[Node("Gradient Color Easing", "Outputs an eased gradient color value, as long as previous and new gradients have the same number of colors",
    "Easing", InputType = typeof(ColorGradient), OutputType = typeof(ColorGradient))]
public class SimpleGradientEasingNode : Node
{
    private DateTime _lastEvaluate = DateTime.Now;

    private readonly ColorGradient _output;

    private float _progress = 1;
    //ArrayLists to reduce resizing and recreation overhead
    private List<SKColor> _currentValue;
    private List<SKColor> _sourceValue;
    private SKColor[] _targetValue;

    public SimpleGradientEasingNode()
    {
        Input = CreateInputPin<ColorGradient>();
        EasingTime = CreateInputPin<Numeric>("delay");
        EasingFunction = CreateInputPin<Easings.Functions>("function");

        Output = CreateOutputPin<ColorGradient>();
        Output.Value = _output = ColorGradient.GetUnicornBarf();
    }

    public InputPin<ColorGradient> Input { get; set; }
    public InputPin<Numeric> EasingTime { get; set; }
    public InputPin<Easings.Functions> EasingFunction { get; set; }

    public OutputPin<ColorGradient> Output { get; set; }

    public override void Evaluate()
    {
        if (Input.Value == null)
        {
            return;
        }
        //first time evaluated
        if (_sourceValue == null)
        {
            _targetValue = Input.Value.GetColorsArray();
            _currentValue = new List<SKColor>(Input.Value.GetColorsArray());
            _sourceValue = new List<SKColor>(Input.Value.GetColorsArray());
        }

        // reset the progress
        if (!_targetValue.Equals(Input.Value.GetColorsArray()))
        {
            _progress = 0f;
            
            _sourceValue.Clear();
            _sourceValue.AddRange(_currentValue);
            
            _targetValue = Input.Value.GetColorsArray();
            ResizeList(_currentValue, _targetValue.Length);
            ResizeList(_sourceValue, _targetValue.Length);

            _output.Resize(_targetValue.Length);
        }

        // Update until finished
        if (_progress < 1f)
        {
            Update();
            for (int i = 0; i < _targetValue.Length; i++)
            {
                _output[i].Color = _currentValue[i];
            }
        }

        Output.Value = _output;

        _lastEvaluate = DateTime.Now;
    }

    private void Update()
    {
        float easingTime = EasingTime.Value != 0f ? EasingTime.Value : 1f;
        TimeSpan delta = DateTime.Now - _lastEvaluate;

        // In case of odd delta's, keep progress between 0f and 1f
        _progress = Math.Clamp(_progress + (float) delta.TotalMilliseconds / easingTime, 0f, 1f);

        float interpolation = (float) Easings.Interpolate(_progress, EasingFunction.Value);
        for (int i = 0; i < _targetValue.Length; i++)
        {
            SKColor sourceColor = _sourceValue[i];
            SKColor targetColor = _targetValue[i];
            SKColor newCurrentColor = sourceColor.Interpolate(targetColor, interpolation);
            _currentValue[i] = newCurrentColor;
        }
    }

    private void ResizeList(List<SKColor> list, int size)
    {
        if (list.Count < size)
        {
            SKColor skColor = SKColor.FromHsv(0, 0, 0, 0);
            for (int index = _targetValue.Length; index < size; index++)
            {
                list.Add(skColor);
            }
        }
    }
}
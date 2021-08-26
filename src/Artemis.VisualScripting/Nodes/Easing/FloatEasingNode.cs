using System;
using Artemis.Core;

namespace Artemis.VisualScripting.Nodes.Easing
{
    [Node("Float Easing", "Outputs an eased float value", "Easing", InputType = typeof(float), OutputType = typeof(float))]
    public class FloatEasingNode : Node
    {
        private DateTime _lastEvaluate = DateTime.MinValue;
        private float _progress;
        private float _currentValue;
        private float _sourceValue;
        private float _targetValue;

        public FloatEasingNode() : base("Float Easing", "Outputs an eased float value")
        {
            Input = CreateInputPin<float>();
            EasingTime = CreateInputPin<float>("delay");
            EasingFunction = CreateInputPin<Easings.Functions>("function");

            Output = CreateOutputPin<float>();
        }

        public InputPin<float> Input { get; set; }
        public InputPin<float> EasingTime { get; set; }
        public InputPin<Easings.Functions> EasingFunction { get; set; }

        public OutputPin<float> Output { get; set; }

        public override void Evaluate()
        {
            DateTime now = DateTime.Now;

            // If the value changed reset progress
            if (Math.Abs(_targetValue - Input.Value) > 0.001f)
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
            TimeSpan delta = DateTime.Now - _lastEvaluate;

            // In case of odd delta's, keep progress between 0f and 1f
            _progress = Math.Clamp(_progress + (float) delta.TotalMilliseconds / EasingTime.Value, 0f, 1f);

            double eased = _sourceValue + (_targetValue - _sourceValue) * Easings.Interpolate(_progress, EasingFunction.Value);
            _currentValue = (float) eased;
        }
    }
}
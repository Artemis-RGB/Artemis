using System;
using Artemis.Core;

namespace Artemis.VisualScripting.Nodes.Easing
{
    [Node("Double Easing", "Outputs an eased double value")]
    public class DoubleEasingNode : Node
    {
        private DateTime _lastEvaluate = DateTime.MinValue;
        private float _progress;
        private double _currentValue;
        private double _sourceValue;
        private double _targetValue;

        public DoubleEasingNode() : base("Double Easing", "Outputs an eased double value")
        {
            Input = CreateInputPin<double>();
            EasingTime = CreateInputPin<float>("delay");
            EasingFunction = CreateInputPin<Easings.Functions>("function");

            Output = CreateOutputPin<double>();
        }

        public InputPin<double> Input { get; set; }
        public InputPin<float> EasingTime { get; set; }
        public InputPin<Easings.Functions> EasingFunction { get; set; }

        public OutputPin<double> Output { get; set; }

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
            _progress = Math.Clamp(_progress + (float)delta.TotalMilliseconds / EasingTime.Value, 0f, 1f);

            double eased = _sourceValue + (_targetValue - _sourceValue) * Easings.Interpolate(_progress, EasingFunction.Value);
            _currentValue = eased;
        }
    }
}
namespace Artemis.Core
{
    internal class SubtractModifierType : DataBindingModifierType<double, double>
    {
        public override string Name => "Subtract";
        public override string Icon => "Minus";

        public override double Apply(double currentValue, double parameterValue)
        {
            return currentValue - parameterValue;
        }
    }
}
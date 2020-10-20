namespace Artemis.Core.DefaultTypes
{
    internal class MultiplicationModifierType : DataBindingModifierType<double, double>
    {
        public override string Name => "Multiply by";
        public override string Icon => "Close";

        public override double Apply(double currentValue, double parameterValue)
        {
            return currentValue * parameterValue;
        }
    }
}
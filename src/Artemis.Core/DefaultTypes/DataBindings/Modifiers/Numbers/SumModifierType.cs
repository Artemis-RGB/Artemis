namespace Artemis.Core.DefaultTypes
{
    internal class SumModifierType : DataBindingModifierType<double, double>
    {
        public override string Name => "Sum";
        public override string Icon => "Plus";

        public override double Apply(double currentValue, double parameterValue)
        {
            return currentValue + parameterValue;
        }
    }
}
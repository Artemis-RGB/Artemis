namespace Artemis.Core
{
    internal class DivideModifierType : DataBindingModifierType<double, double>
    {
        public override string Name => "Divide by";
        public override string Icon => "Divide";

        public override double Apply(double currentValue, double parameterValue)
        {
            // Ye ye none of that
            if (parameterValue == 0)
                return 0;

            return currentValue / parameterValue;
        }
    }
}
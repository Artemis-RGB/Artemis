namespace Artemis.Core
{
    internal class PercentageOfModifierType : DataBindingModifierType<double, double>
    {
        public override string Name => "Percentage of";
        public override string Icon => "Percent";
        public override string Description => "Calculates how much percent the parameter value is of the current value";

        public override double Apply(double currentValue, double parameterValue)
        {
            // Ye ye none of that
            if (parameterValue == 0d)
                return 100d;

            return 100d / parameterValue * currentValue;
        }
    }
}
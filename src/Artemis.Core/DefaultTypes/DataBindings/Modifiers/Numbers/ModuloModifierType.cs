namespace Artemis.Core
{
    internal class ModuloModifierType : DataBindingModifierType<double, double>
    {
        public override string Name => "Modulo";
        public override string Icon => "Stairs";
        public override string Category => "Advanced";
        public override string Description => "Calculates the remained of the division between the input value and the parameter";

        public override double Apply(double currentValue, double parameterValue)
        {
            return currentValue % parameterValue;
        }
    }
}
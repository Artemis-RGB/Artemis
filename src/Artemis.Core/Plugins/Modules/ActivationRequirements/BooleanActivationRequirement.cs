namespace Artemis.Core.Plugins.Modules.ActivationRequirements
{
    /// <summary>
    ///     Evaluates to true or false by returning the value of ActivationMet
    /// </summary>
    public class BooleanActivationRequirement : IModuleActivationRequirement
    {
        /// <summary>
        ///     Gets or sets whether the activation requirement is met
        /// </summary>
        public bool ActivationMet { get; set; }

        /// <inheritdoc />
        public bool Evaluate()
        {
            return ActivationMet;
        }
    }
}
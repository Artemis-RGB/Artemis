namespace Artemis.Core.Plugins.Modules.ActivationRequirements
{
    /// <summary>
    ///     Evaluates to true or false by checking requirements specific to the implementation
    /// </summary>
    public interface IModuleActivationRequirement
    {
        /// <summary>
        ///     Called to determine whether the activation requirement is met
        /// </summary>
        /// <returns></returns>
        bool Evaluate();

        /// <summary>
        ///     Returns a user-friendly description of the activation requirement, should include parameters if applicable
        /// </summary>
        /// <returns>A user-friendly description of the activation requirement</returns>
        string GetUserFriendlyDescription();
    }
}
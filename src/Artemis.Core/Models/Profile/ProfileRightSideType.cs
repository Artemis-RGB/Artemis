namespace Artemis.Core.Models.Profile
{
    /// <summary>
    ///     An enum defining the right side type of a profile entity
    /// </summary>
    public enum ProfileRightSideType
    {
        /// <summary>
        ///     A static right side value
        /// </summary>
        Static,

        /// <summary>
        ///     A dynamic right side value based on a path in a data model
        /// </summary>
        Dynamic
    }
}
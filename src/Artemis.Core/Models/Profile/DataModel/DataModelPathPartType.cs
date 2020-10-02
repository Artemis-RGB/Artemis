namespace Artemis.Core
{
    /// <summary>
    ///     Represents a type of data model path
    /// </summary>
    public enum DataModelPathPartType
    {
        /// <summary>
        ///     Represents an invalid data model type that points to a missing data model
        /// </summary>
        Invalid,

        /// <summary>
        ///     Represents a static data model type that points to a data model defined in code
        /// </summary>
        Static,

        /// <summary>
        ///     Represents a static data model type that points to a data model defined at runtime
        /// </summary>
        Dynamic
    }
}
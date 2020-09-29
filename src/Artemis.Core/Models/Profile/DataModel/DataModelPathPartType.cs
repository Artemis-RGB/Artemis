namespace Artemis.Core
{
    /// <summary>
    ///     Represents a type of data model path
    /// </summary>
    public enum DataModelPathPartType
    {
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
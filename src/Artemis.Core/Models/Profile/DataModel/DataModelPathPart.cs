namespace Artemis.Core
{
    /// <summary>
    ///     Represents a part of a data model path
    /// </summary>
    public class DataModelPathPart
    {
        /// <summary>
        ///     Gets the identifier that is associated with this part
        /// </summary>
        public string Identifier { get; private set; }

        /// <summary>
        ///     Gets the type of data model this part of the path points to
        /// </summary>
        public DataModelPathPartType Type { get; private set; }
    }
}
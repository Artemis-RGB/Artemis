namespace Artemis.UI.Shared
{
    /// <summary>
    ///     Represents a configuration to use while updating a <see cref="DataModelVisualizationViewModel" />
    /// </summary>
    public class DataModelUpdateConfiguration
    {
        /// <summary>
        ///     Creates a new instance of the <see cref="DataModelUpdateConfiguration" /> class
        /// </summary>
        /// <param name="createEventChildren">A boolean indicating whether or not event children should be created</param>
        public DataModelUpdateConfiguration(bool createEventChildren)
        {
            CreateEventChildren = createEventChildren;
        }

        /// <summary>
        ///     Gets a boolean indicating whether or not event children should be created
        /// </summary>
        public bool CreateEventChildren { get; }
    }
}
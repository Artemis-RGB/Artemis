namespace Artemis.UI.Shared.DataModelVisualization.Shared;

/// <summary>
///     Represents a configuration to use while updating a <see cref="DataModelVisualizationViewModel" />
/// </summary>
public class DataModelUpdateConfiguration
{
    /// <summary>
    ///     Creates a new instance of the <see cref="DataModelUpdateConfiguration" /> class
    /// </summary>
    /// <param name="createEventChildren">A boolean indicating whether event children should be created</param>
    /// <param name="updateAllChildren">A boolean indicating whether all children should be updated</param>
    public DataModelUpdateConfiguration(bool createEventChildren, bool updateAllChildren)
    {
        CreateEventChildren = createEventChildren;
        UpdateAllChildren = updateAllChildren;
    }

    /// <summary>
    ///     Gets a boolean indicating whether event children should be created
    /// </summary>
    public bool CreateEventChildren { get; }

    /// <summary>
    ///     Gets a boolean indicating whether all children should be updated
    /// </summary>
    public bool UpdateAllChildren { get; }
}
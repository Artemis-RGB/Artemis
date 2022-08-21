namespace Artemis.Core;

/// <summary>
///     Represents a model that updates using a delta time
/// </summary>
public interface IUpdateModel
{
    /// <summary>
    ///     Performs an update on the model
    /// </summary>
    /// <param name="timeline">The timeline to apply during update</param>
    void Update(Timeline timeline);
}
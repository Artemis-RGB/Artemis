namespace Artemis.Core;

/// <summary>
///     Represents a plugin related help page
/// </summary>
public interface IPluginHelpPage
{
    /// <summary>
    ///     Gets the plugin the help page belongs to.
    /// </summary>
    Plugin Plugin { get; }

    /// <summary>
    ///     Gets the title of the help page.
    /// </summary>
    public string Title { get; }

    /// <summary>
    ///     An ID used to quickly lead users to the help page in case of an error.
    /// </summary>
    public string Id { get; }
}
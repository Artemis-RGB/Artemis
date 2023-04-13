using System.IO;

namespace Artemis.Core;

/// <summary>
///     Represents a plugin related help page
/// </summary>
public class MarkdownPluginHelpPage : IPluginHelpPage
{
    /// <summary>
    ///     Creates a new instance of the <see cref="MarkdownPluginHelpPage" /> class.
    /// </summary>
    /// <param name="plugin">The plugin to display the markdown for.</param>
    /// <param name="markdownFile">A file path relative to the plugin or absolute, pointing to the markdown to display</param>
    /// <param name="id">The ID of the help page, used to quickly lead users to it in case of errors.</param>
    /// <exception cref="FileNotFoundException"></exception>
    public MarkdownPluginHelpPage(Plugin plugin, string title, string id, string markdownFile)
    {
        Plugin = plugin;
        Title = title;
        Id = id;
        MarkdownFile = Path.IsPathRooted(markdownFile) ? markdownFile : Plugin.ResolveRelativePath(markdownFile);

        if (!File.Exists(MarkdownFile))
            throw new FileNotFoundException($"Could not find markdown file at \"{MarkdownFile}\"");
    }

    /// <inheritdoc />
    public Plugin Plugin { get; }

    /// <inheritdoc />
    public string Title { get; }

    /// <inheritdoc />
    public string Id { get; }

    /// <summary>
    ///     Gets the absolute path to the markdown that is to be displayed.
    /// </summary>
    public string MarkdownFile { get; }
}
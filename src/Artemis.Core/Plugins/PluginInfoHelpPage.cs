using Newtonsoft.Json;

namespace Artemis.Core;

/// <summary>
///     Represents basic info about a plugin and contains a reference to the instance of said plugin
/// </summary>
internal class PluginInfoHelpPage
{
    [JsonConstructor]
    public PluginInfoHelpPage(string title, string markdownFile, string id)
    {
        Title = title;
        MarkdownFile = markdownFile;
        Id = id;
    }

    [JsonProperty(Required = Required.Always)]
    public string Title { get; }

    [JsonProperty(Required = Required.Always)]
    public string MarkdownFile { get; }

    [JsonProperty(Required = Required.Always)]
    public string Id { get; }
}
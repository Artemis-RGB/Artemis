using Artemis.Core;

namespace Artemis.WebClient.Workshop.Handlers.UploadHandlers;

public class PluginEntrySource : IEntrySource
{
    public PluginEntrySource(PluginInfo pluginInfo, string path)
    {
        PluginInfo = pluginInfo;
        Path = path;
    }

    public PluginInfo PluginInfo { get; set; }
    public string Path { get; set; }
}
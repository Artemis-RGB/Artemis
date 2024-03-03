using Artemis.Core;

namespace Artemis.WebClient.Workshop.Handlers.UploadHandlers;

public class ProfileEntrySource : IEntrySource
{
    public ProfileEntrySource(ProfileConfiguration profileConfiguration, List<PluginFeature> dependencies)
    {
        ProfileConfiguration = profileConfiguration;
        Dependencies = dependencies;
    }

    public ProfileConfiguration ProfileConfiguration { get; }
    public List<PluginFeature> Dependencies { get; }
}
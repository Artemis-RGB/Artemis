using Artemis.Core;

namespace Artemis.WebClient.Workshop.Handlers.UploadHandlers;

public class ProfileEntrySource : IEntrySource
{
    public ProfileEntrySource(ProfileConfiguration profileConfiguration)
    {
        ProfileConfiguration = profileConfiguration;
    }

    public ProfileConfiguration ProfileConfiguration { get; set; }
}
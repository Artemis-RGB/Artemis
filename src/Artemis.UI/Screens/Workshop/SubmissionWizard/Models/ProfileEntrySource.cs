using Artemis.Core;

namespace Artemis.UI.Screens.Workshop.SubmissionWizard.Models;

public class ProfileEntrySource : IEntrySource
{
    public ProfileEntrySource(ProfileConfiguration profileConfiguration)
    {
        ProfileConfiguration = profileConfiguration;
    }

    public ProfileConfiguration ProfileConfiguration { get; set; }
}
using Artemis.Core;
using Artemis.UI.Services.Updating;
using Artemis.UI.Shared;
using Artemis.WebClient.Updating;

namespace Artemis.UI.Screens.Settings.Updating;

public class ReleaseViewModel : ViewModelBase
{
    private readonly IUpdateService _updateService;
    public IGetReleases_PublishedReleases_Nodes Release { get; }

    public ReleaseViewModel(IUpdateService updateService, IGetReleases_PublishedReleases_Nodes release)
    {
        _updateService = updateService;
        Release = release;
    }
    
    public bool IsCurrentVersion => Release.Version == Constants.CurrentVersion;
    public bool IsPreviousVersion => Release.Version == _updateService.PreviousVersion;
    public bool ShowStatusIndicator => IsCurrentVersion || IsPreviousVersion;
}
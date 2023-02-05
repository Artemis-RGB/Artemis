using Artemis.Core;
using Artemis.UI.Shared;

namespace Artemis.UI.Screens.Settings.Updating;

public class UpdateInstallationViewModel : DialogViewModelBase<bool>
{
    private readonly string _nextReleaseId;

    public UpdateInstallationViewModel(string nextReleaseId)
    {
        _nextReleaseId = nextReleaseId;
    }
}
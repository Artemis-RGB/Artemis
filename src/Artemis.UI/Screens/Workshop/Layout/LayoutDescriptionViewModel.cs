using Artemis.UI.Screens.Workshop.EntryReleases;
using Artemis.UI.Shared.Routing;
using Artemis.WebClient.Workshop;
using PropertyChanged.SourceGenerator;

namespace Artemis.UI.Screens.Workshop.Layout;

public partial class LayoutDescriptionViewModel : RoutableScreen
{
    [Notify] private IEntryDetails? _entry;

    public LayoutDescriptionViewModel(EntryReleaseInfoViewModel releaseInfoViewModel)
    {
        ReleaseInfoViewModel = releaseInfoViewModel;
        ReleaseInfoViewModel.InDetailsScreen = false;
    }

    public EntryReleaseInfoViewModel ReleaseInfoViewModel { get; }
}
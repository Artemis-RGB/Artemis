using Artemis.UI.Shared.Routing;
using Artemis.WebClient.Workshop;
using PropertyChanged.SourceGenerator;

namespace Artemis.UI.Screens.Workshop.Layout;

public partial class LayoutDescriptionViewModel : RoutableScreen
{
    [Notify] private IEntryDetails? _entry;
}
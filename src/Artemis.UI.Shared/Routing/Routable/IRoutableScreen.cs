using System.Threading;
using System.Threading.Tasks;
using ReactiveUI;

namespace Artemis.UI.Shared.Routing;

/// <summary>
///     For internal use.
/// </summary>
internal interface IRoutableScreen : IActivatableViewModel
{
    Task InternalOnNavigating(NavigationArguments args, CancellationToken cancellationToken);
    Task InternalOnClosing(NavigationArguments args);
}
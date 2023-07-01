using System.Threading;
using System.Threading.Tasks;
using ReactiveUI;

namespace Artemis.UI.Shared.Routing;

/// <summary>
/// For internal use.
/// </summary>
/// <seealso cref="RoutableScreen{TScreen}"/>
/// <seealso cref="RoutableScreen{TScreen, TParam}"/>
internal interface IRoutableScreen : IActivatableViewModel
{
    /// <summary>
    /// Gets or sets a value indicating whether or not to reuse the child screen instance if the type has not changed.
    /// </summary>
    /// <remarks>Defaults to <see langword="true"/>.</remarks>
    bool RecycleScreen { get; }

    object? InternalScreen { get; }
    void InternalChangeScreen(object? screen);
    Task InternalOnNavigating(NavigationArguments args, CancellationToken cancellationToken);
    Task InternalOnClosing(NavigationArguments args);
}
namespace Artemis.UI.Shared.Routing;

/// <summary>
///     For internal use.
/// </summary>
/// <seealso cref="RoutableHostScreen{TScreen}" />
/// <seealso cref="RoutableHostScreen{TScreen,TParam}" />
internal interface IRoutableHostScreen : IRoutableScreen
{
    bool RecycleScreen { get; }
    IRoutableScreen? InternalScreen { get; }
    IRoutableScreen? InternalDefaultScreen { get; }
    void InternalChangeScreen(IRoutableScreen? screen);
}
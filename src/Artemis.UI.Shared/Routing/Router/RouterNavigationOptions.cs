namespace Artemis.UI.Shared.Routing;

/// <summary>
/// Represents navigation options used to control navigation behaviour.
/// </summary>
public class RouterNavigationOptions
{
    /// <summary>
    /// Gets or sets a boolean indicating whether or not to add the navigation to the history.
    /// </summary>
    public bool AddToHistory { get; set; } = true;

    /// <summary>
    /// Gets or sets a boolean indicating whether or not to recycle already active screens.
    /// </summary>
    public bool RecycleScreens { get; set; } = true;

    /// <summary>
    /// Gets or sets a boolean indicating whether route changes should be ignored if they are a partial match.
    /// </summary>
    /// <example>If set to true, a route change from <c>page/subpage1/subpage2</c> to <c>page/subpage1</c> will be ignored.</example>
    public bool IgnoreOnPartialMatch { get; set; } = false;
}
using System;

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

    /// <summary>
    /// Gets or sets the path to use when determining whether the path is a partial match,
    /// only has any effect if <see cref="IgnoreOnPartialMatch"/> is <see langword="true"/>.
    /// </summary>
    public string? PartialMatchOverride { get; set; }

    /// <summary>
    /// Gets or sets a boolean value indicating whether logging should be enabled.
    /// <remarks>Errors and warnings are always logged.</remarks>
    /// </summary>
    public bool EnableLogging { get; set; } = true;

    /// <summary>
    /// Gets or sets any additional arguments to pass to the screen.
    /// </summary>
    public object? AdditionalArguments { get; set; }

    /// <summary>
    /// Determines whether the given two paths are considered equal using these navigation options.
    /// </summary>
    /// <param name="current">The current path.</param>
    /// <param name="target">The target path.</param>
    /// <returns><see langword="true"/> if the paths are considered equal; otherwise <see langword="false"/>.</returns>
    internal bool PathEquals(string current, string target)
    {
        if (PartialMatchOverride != null && IgnoreOnPartialMatch)
            target = PartialMatchOverride;
        
        if (IgnoreOnPartialMatch)
            return current.StartsWith(target, StringComparison.InvariantCultureIgnoreCase);
        return string.Equals(current, target, StringComparison.InvariantCultureIgnoreCase);
    }
}
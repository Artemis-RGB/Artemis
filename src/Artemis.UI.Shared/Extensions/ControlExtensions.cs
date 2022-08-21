using Avalonia.Controls;
using Avalonia.LogicalTree;

namespace Artemis.UI.Shared.Extensions;

/// <summary>
///     Provides extension methods for Avalonia's <see cref="Control" /> type
/// </summary>
public static class ControlExtensions
{
    /// <summary>
    ///     Clears all data validation errors on the given control and any of it's logical siblings
    /// </summary>
    /// <param name="target">The target control</param>
    public static void ClearAllDataValidationErrors(this Control target)
    {
        DataValidationErrors.ClearErrors(target);
        foreach (ILogical logicalChild in target.GetLogicalChildren())
        {
            if (logicalChild is Control childControl)
                childControl.ClearAllDataValidationErrors();
        }
    }
}
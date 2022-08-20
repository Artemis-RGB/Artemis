using System.Collections.Generic;
using Artemis.Core;

namespace Artemis.UI.Shared.Providers;

/// <summary>
///     Represents a provider for color gradient storage.
/// </summary>
public interface IColorGradientStorageProvider
{
    /// <summary>
    ///     Returns a list containing a copy of all stored color gradients.
    /// </summary>
    /// <returns>A <see cref="List{T}" /> of <see cref="ColorGradient" /> containing a copy of all stored color gradients.</returns>
    public List<ColorGradient> GetColorGradients();

    /// <summary>
    ///     Saves the provided color gradient to storage.
    /// </summary>
    /// <param name="colorGradient">The color gradient to save.</param>
    public void SaveColorGradient(ColorGradient colorGradient);

    /// <summary>
    ///     Deletes the provided color gradient from storage.
    /// </summary>
    /// <param name="colorGradient">The color gradient to delete.</param>
    public void DeleteColorGradient(ColorGradient colorGradient);
}
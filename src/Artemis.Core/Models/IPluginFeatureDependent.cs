using System.Collections.Generic;

namespace Artemis.Core;

/// <summary>
/// Represents a class that depends on plugin features
/// </summary>
public interface IPluginFeatureDependent
{
    /// <summary>
    /// Gets the plugin features this class depends on, may contain the same plugin feature twice if depending on it in multiple ways.
    /// </summary>
    /// <returns>A <see cref="List{T}"/> of <see cref="PluginFeature"/> this class depends on.</returns>
    public IEnumerable<PluginFeature> GetFeatureDependencies();
}
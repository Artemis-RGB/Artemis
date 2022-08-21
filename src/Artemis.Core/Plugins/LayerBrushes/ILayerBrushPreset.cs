namespace Artemis.Core.LayerBrushes;

/// <summary>
///     Represents a brush preset for a brush.
/// </summary>
public interface ILayerBrushPreset
{
    /// <summary>
    ///     Gets the name of the preset
    /// </summary>
    string Name { get; }

    /// <summary>
    ///     Gets the description of the preset
    /// </summary>
    string Description { get; }

    /// <summary>
    ///     Gets the icon of the preset
    /// </summary>
    string Icon { get; }

    /// <summary>
    ///     Applies the preset to the layer brush
    /// </summary>
    void Apply();
}
using System.Collections.Generic;
using System.Linq;
using Artemis.Core;

namespace Artemis.UI.Shared.Services.ProfileEditor.Commands;

/// <summary>
///     Represents a profile editor command that can be used to update a color gradient.
/// </summary>
public class UpdateColorGradient : IProfileEditorCommand
{
    private readonly ColorGradient _colorGradient;
    private readonly List<ColorGradientStop> _originalStops;
    private readonly List<ColorGradientStop> _stops;

    /// <summary>
    ///     Creates a new instance of the <see cref="UpdateColorGradient" /> class.
    /// </summary>
    public UpdateColorGradient(ColorGradient colorGradient, List<ColorGradientStop> stops, List<ColorGradientStop>? originalStops)
    {
        _colorGradient = colorGradient;
        _stops = stops.Select(s => new ColorGradientStop(s.Color, s.Position)).ToList();
        _originalStops = originalStops ?? _colorGradient.Select(s => new ColorGradientStop(s.Color, s.Position)).ToList();
    }

    #region Implementation of IProfileEditorCommand

    /// <inheritdoc />
    public string DisplayName => "Update color gradient";

    /// <inheritdoc />
    public void Execute()
    {
        ApplyStops(_stops);
    }

    /// <inheritdoc />
    public void Undo()
    {
        ApplyStops(_originalStops);
    }

    #endregion

    private void ApplyStops(List<ColorGradientStop> stops)
    {
        while (_colorGradient.Count > stops.Count)
            _colorGradient.RemoveAt(_colorGradient.Count - 1);

        for (int index = 0; index < stops.Count; index++)
        {
            ColorGradientStop colorGradientStop = stops[index];
            // Add missing color gradients
            if (index >= _colorGradient.Count)
            {
                _colorGradient.Add(new ColorGradientStop(colorGradientStop.Color, colorGradientStop.Position));
            }
            // Update existing color gradients
            else
            {
                _colorGradient[index].Color = colorGradientStop.Color;
                _colorGradient[index].Position = colorGradientStop.Position;
            }
        }
    }
}
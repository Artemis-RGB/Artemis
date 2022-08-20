using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.Core;

namespace Artemis.UI.Shared.Services.ProfileEditor.Commands;

/// <summary>
///     Represents a profile editor command that can be used to update a layer property of type <typeparamref name="T" />.
/// </summary>
public class UpdateColorGradient : IProfileEditorCommand
{
    private readonly ColorGradient _colorGradient;
    private readonly List<ColorGradientStop> _stops;
    private readonly List<ColorGradientStop> _originalStops;

    /// <summary>
    ///     Creates a new instance of the <see cref="UpdateColorGradient" /> class.
    /// </summary>
    public UpdateColorGradient(ColorGradient colorGradient, List<ColorGradientStop> stops, List<ColorGradientStop>? originalStops)
    {
        _colorGradient = colorGradient;
        _stops = stops;
        _originalStops = originalStops ?? _colorGradient.Select(s => new ColorGradientStop(s.Color, s.Position)).ToList();
    }

    #region Implementation of IProfileEditorCommand

    /// <inheritdoc />
    public string DisplayName => "Update color gradient";

    /// <inheritdoc />
    public void Execute()
    {
        _colorGradient.Clear();
        foreach (ColorGradientStop colorGradientStop in _stops)
            _colorGradient.Add(colorGradientStop);
    }

    /// <inheritdoc />
    public void Undo()
    {
        _colorGradient.Clear();
        foreach (ColorGradientStop colorGradientStop in _originalStops)
            _colorGradient.Add(colorGradientStop);
    }

    #endregion
}
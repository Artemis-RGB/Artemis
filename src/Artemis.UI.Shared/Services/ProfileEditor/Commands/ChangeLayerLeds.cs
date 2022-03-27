using System.Collections.Generic;
using Artemis.Core;

namespace Artemis.UI.Shared.Services.ProfileEditor.Commands;

/// <summary>
///     Represents a profile editor command that can be used to change the LEDs of a layer.
/// </summary>
public class ChangeLayerLeds : IProfileEditorCommand
{
    private readonly Layer _layer;
    private readonly List<ArtemisLed> _leds;
    private readonly List<ArtemisLed> _originalLeds;

    /// <summary>
    ///     Creates a new instance of the <see cref="ChangeLayerLeds" /> class.
    /// </summary>
    public ChangeLayerLeds(Layer layer, List<ArtemisLed> leds)
    {
        _layer = layer;
        _leds = leds;
        _originalLeds = new List<ArtemisLed>(_layer.Leds);
    }

    #region Implementation of IProfileEditorCommand

    /// <inheritdoc />
    public string DisplayName => "Change layer LEDs";

    /// <inheritdoc />
    public void Execute()
    {
        _layer.ClearLeds();
        _layer.AddLeds(_leds);
    }

    /// <inheritdoc />
    public void Undo()
    {
        _layer.ClearLeds();
        _layer.AddLeds(_originalLeds);
    }

    #endregion
}
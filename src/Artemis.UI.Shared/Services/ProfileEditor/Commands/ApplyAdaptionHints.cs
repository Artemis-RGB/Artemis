using System.Collections.Generic;
using Artemis.Core;

namespace Artemis.UI.Shared.Services.ProfileEditor.Commands;

/// <summary>
///     Represents a profile editor command that can be used to apply adaption hints to a layer.
/// </summary>
public class ApplyAdaptionHints : IProfileEditorCommand
{
    private readonly Layer _layer;
    private readonly List<ArtemisDevice> _devices;
    private readonly List<ArtemisLed> _originalLeds;

    /// <summary>
    ///     Creates a new instance of the <see cref="ApplyAdaptionHints" /> class.
    /// </summary>
    public ApplyAdaptionHints(Layer layer, List<ArtemisDevice> devices)
    {
        _layer = layer;
        _devices = devices;
        _originalLeds = new List<ArtemisLed>(_layer.Leds);
    }

    #region Implementation of IProfileEditorCommand

    /// <inheritdoc />
    public string DisplayName => "Apply adaption hints";

    /// <inheritdoc />
    public void Execute()
    {
        _layer.ClearLeds();
        _layer.Adapter.Adapt(_devices);
    }

    /// <inheritdoc />
    public void Undo()
    {
        _layer.ClearLeds();
        _layer.AddLeds(_originalLeds);
    }

    #endregion
}
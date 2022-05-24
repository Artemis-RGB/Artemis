using System;

namespace Artemis.Core;

/// <summary>
///     Provides data for layer adapter hint events.
/// </summary>
public class LayerAdapterHintEventArgs : EventArgs
{
    internal LayerAdapterHintEventArgs(IAdaptionHint adaptionHint)
    {
        AdaptionHint = adaptionHint;
    }

    /// <summary>
    ///     Gets the layer adaption hint this event is related to
    /// </summary>
    public IAdaptionHint AdaptionHint { get; }
}
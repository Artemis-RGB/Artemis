using System;
using Artemis.Core.LayerBrushes;

namespace Artemis.UI.Shared.LayerBrushes
{
    /// <inheritdoc />
    public class LayerBrushConfigurationDialog<T> : LayerBrushConfigurationDialog where T : BrushConfigurationViewModel
    {
        /// <inheritdoc />
        public override Type Type => typeof(T);
    }

    /// <summary>
    ///     Describes a UI tab for a layer brush
    /// </summary>
    public abstract class LayerBrushConfigurationDialog : ILayerBrushConfigurationDialog
    {
        /// <summary>
        ///     The type of view model the tab contains
        /// </summary>
        public abstract Type Type { get; }
    }
}
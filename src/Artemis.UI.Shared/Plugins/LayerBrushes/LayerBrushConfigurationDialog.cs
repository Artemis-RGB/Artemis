using System;
using Artemis.Core.LayerBrushes;

namespace Artemis.UI.Shared.LayerBrushes
{
    /// <inheritdoc />
    public class LayerBrushConfigurationDialog<T> : LayerBrushConfigurationDialog where T : BrushConfigurationViewModel
    {
        /// <inheritdoc />
        public LayerBrushConfigurationDialog()
        {
        }

        /// <inheritdoc />
        public LayerBrushConfigurationDialog(int dialogWidth, int dialogHeight)
        {
            DialogWidth = dialogWidth;
            DialogHeight = dialogHeight;
        }

        /// <inheritdoc />
        public override Type Type => typeof(T);
    }

    /// <summary>
    ///     Describes a UI tab for a layer brush
    /// </summary>
    public abstract class LayerBrushConfigurationDialog : ILayerBrushConfigurationDialog
    {
        /// <summary>
        /// The default width of the dialog
        /// </summary>
        public int DialogWidth { get; set; } = 800;

        /// <summary>
        /// The default height of the dialog
        /// </summary>
        public int DialogHeight { get; set; } = 800;

        /// <summary>
        ///     The type of view model the tab contains
        /// </summary>
        public abstract Type Type { get; }
    }
}
using System;
using Artemis.Core.LayerEffects;

namespace Artemis.UI.Shared.LayerEffects
{
    /// <inheritdoc />
    public class LayerEffectConfigurationDialog<T> : LayerEffectConfigurationDialog where T : EffectConfigurationViewModel
    {
        /// <inheritdoc />
        public override Type Type => typeof(T);
    }

    /// <summary>
    ///     Describes a UI tab for a specific layer effect
    /// </summary>
    public abstract class LayerEffectConfigurationDialog : ILayerEffectConfigurationDialog
    {
        // TODO: See if this is still in use
        /// <summary>
        ///     The layer effect this dialog belongs to
        /// </summary>
        public BaseLayerEffect? LayerEffect { get; set; }

        /// <summary>
        ///     The type of view model the tab contains
        /// </summary>
        public abstract Type Type { get; }
    }
}
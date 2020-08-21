using System;
using Artemis.Core.Plugins.Abstract.ViewModels;
using Artemis.Core.Plugins.LayerEffect.Abstract;

namespace Artemis.Core.Plugins.LayerEffect
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
    public abstract class LayerEffectConfigurationDialog
    {
        /// <summary>
        ///     The layer effect this dialog belongs to
        /// </summary>
        internal BaseLayerEffect LayerEffect { get; set; }

        /// <summary>
        ///     The type of view model the tab contains
        /// </summary>
        public abstract Type Type { get; }
    }
}
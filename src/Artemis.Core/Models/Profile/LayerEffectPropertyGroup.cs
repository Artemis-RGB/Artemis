namespace Artemis.Core
{
    /// <summary>
    ///     Represents a property group on a layer
    ///     <para>
    ///         Note: You cannot initialize property groups yourself. If properly placed and annotated, the Artemis core will
    ///         initialize these for you.
    ///     </para>
    /// </summary>
    public abstract class LayerEffectPropertyGroup : LayerPropertyGroup
    {
        /// <summary>
        ///     Whether or not this layer effect is enabled
        /// </summary>
        [PropertyDescription(Name = "Enabled", Description = "Whether or not this layer effect is enabled")]
        public BoolLayerProperty IsEnabled { get; set; } = null!;

        internal void InitializeIsEnabled()
        {
            IsEnabled.DefaultValue = true;
            if (!IsEnabled.IsLoadedFromStorage)
                IsEnabled.SetCurrentValue(true);
        }
    }
}
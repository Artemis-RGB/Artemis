namespace Artemis.Core.ScriptingProviders
{
    /// <summary>
    ///     Represents a script bound to a specific <see cref="LayerProperty{T}" /> processed by a
    ///     <see cref="ScriptingProvider" />.
    /// </summary>
    public abstract class PropertyScript : Script
    {
        /// <inheritdoc />
        protected PropertyScript(ILayerProperty layerProperty, ScriptConfiguration configuration) : base(configuration)
        {
            LayerProperty = layerProperty;
            lock (LayerProperty.Scripts)
            {
                LayerProperty.Scripts.Add(this);
            }
        }

        /// <summary>
        ///     Gets the layer property this script is bound to
        /// </summary>
        public ILayerProperty LayerProperty { get; }

        /// <summary>
        ///     Called whenever the property is about to update
        /// </summary>
        /// <param name="deltaTime">Seconds passed since last update</param>
        public virtual void OnPropertyUpdating(double deltaTime)
        {
        }

        /// <summary>
        ///     Called whenever the property has been updated
        /// </summary>
        /// <param name="deltaTime">Seconds passed since last update</param>
        public virtual void OnPropertyUpdated(double deltaTime)
        {
        }

        #region Overrides of Script

        /// <inheritdoc />
        internal override void InternalCleanup()
        {
            lock (LayerProperty.Scripts)
            {
                LayerProperty.Scripts.Remove(this);    
            }
        }

        #endregion
    }
}
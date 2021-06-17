namespace Artemis.Core.ScriptingProviders
{
    /// <summary>
    ///     Represents a script bound to a specific <see cref="LayerProperty{T}" /> processed by a
    ///     <see cref="ScriptingProvider" />.
    /// </summary>
    public abstract class PropertyScript : Script
    {
        /// <summary>
        ///     Creates a new instance of the <see cref="PropertyScript" /> class.
        /// </summary>
        /// <param name="layerProperty"></param>
        protected PropertyScript(ILayerProperty layerProperty)
        {
            LayerProperty = layerProperty;
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
    }
}
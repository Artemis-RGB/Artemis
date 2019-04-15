using Artemis.Core.Plugins.Models;

namespace Artemis.Core.Plugins.Abstract
{
    /// <inheritdoc />
    /// <summary>
    ///     Allows you to expand the application-wide datamodel
    /// </summary>
    public abstract class DataModelExpansion : Plugin
    {
        protected DataModelExpansion(PluginInfo pluginInfo) : base(pluginInfo)
        {
        }

        public abstract void Update(double deltaTime);
    }
}
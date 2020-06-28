using Artemis.Core.Plugins.Abstract.DataModels.Attributes;
using Artemis.Core.Plugins.Models;

namespace Artemis.Core.Plugins.Abstract.DataModels
{
    public abstract class DataModel
    {
        /// <summary>
        ///     Gets the plugin info this data model belongs to
        /// </summary>
        [DataModelIgnore]
        public PluginInfo PluginInfo { get; internal set; }

        /// <summary>
        ///     Gets the <see cref="DataModelPropertyAttribute" /> describing this data model
        /// </summary>
        [DataModelIgnore]
        public DataModelPropertyAttribute DataModelDescription { get; internal set; }
    }
}
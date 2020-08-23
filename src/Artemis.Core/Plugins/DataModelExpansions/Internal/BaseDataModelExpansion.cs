using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using Artemis.Core.Plugins.DataModelExpansions.Attributes;

namespace Artemis.Core.Plugins.DataModelExpansions.Internal
{
    /// <summary>
    ///     For internal use only, to implement your own layer property type, extend <see cref="DataModelExpansion{T}" />
    ///     instead.
    /// </summary>
    public abstract class BaseDataModelExpansion : Plugin
    {
        protected readonly List<PropertyInfo> HiddenPropertiesList = new List<PropertyInfo>();

        /// <summary>
        ///     Gets a list of all properties ignored at runtime using IgnoreProperty(x => x.y)
        /// </summary>
        public ReadOnlyCollection<PropertyInfo> HiddenProperties => HiddenPropertiesList.AsReadOnly();

        internal DataModel InternalDataModel { get; set; }
        public abstract void Update(double deltaTime);

        /// <summary>
        ///     Override to provide your own data model description. By default this returns a description matching your plugin
        ///     name and description
        /// </summary>
        /// <returns></returns>
        public virtual DataModelPropertyAttribute GetDataModelDescription()
        {
            return new DataModelPropertyAttribute {Name = PluginInfo.Name, Description = PluginInfo.Description};
        }
    }
}
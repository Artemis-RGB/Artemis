using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;

namespace Artemis.Core.DataModelExpansions
{
    /// <summary>
    ///     For internal use only, to implement your own layer property type, extend <see cref="DataModelExpansion{T}" />
    ///     instead.
    /// </summary>
    public abstract class BaseDataModelExpansion : DataModelPluginFeature
    {
        /// <summary>
        ///     Gets a list of all properties ignored at runtime using <c>IgnoreProperty(x => x.y)</c>
        /// </summary>
        protected internal readonly List<PropertyInfo> HiddenPropertiesList = new();

        /// <summary>
        ///     Gets a list of all properties ignored at runtime using <c>IgnoreProperty(x => x.y)</c>
        /// </summary>
        public ReadOnlyCollection<PropertyInfo> HiddenProperties => HiddenPropertiesList.AsReadOnly();

        internal DataModel? InternalDataModel { get; set; }

        /// <summary>
        ///     Called each frame when the data model should update
        /// </summary>
        /// <param name="deltaTime">Time in seconds since the last update</param>
        public abstract void Update(double deltaTime);

        internal void InternalUpdate(double deltaTime)
        {
            if (InternalDataModel != null)
                Update(deltaTime);
        }

        /// <summary>
        ///     Override to provide your own data model description. By default this returns a description matching your plugin
        ///     name and description
        /// </summary>
        /// <returns></returns>
        public virtual DataModelPropertyAttribute GetDataModelDescription()
        {
            return new() {Name = Plugin.Info.Name, Description = Plugin.Info.Description};
        }
    }
}
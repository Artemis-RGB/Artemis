using Artemis.Core.Plugins.Abstract.DataModels;
using Artemis.Core.Plugins.Abstract.DataModels.Attributes;

namespace Artemis.Core.Plugins.Abstract
{
    /// <summary>
    ///     Allows you to expand the application-wide datamodel
    /// </summary>
    public abstract class BaseDataModelExpansion<T> : BaseDataModelExpansion where T : DataModel
    {
        /// <summary>
        ///     The data model driving this module
        /// </summary>
        public T DataModel
        {
            get => (T) InternalDataModel;
            internal set => InternalDataModel = value;
        }
    }

    /// <summary>
    ///     For internal use only, to implement your own layer property type, extend <see cref="BaseDataModelExpansion{T}" />
    ///     instead.
    /// </summary>
    public abstract class BaseDataModelExpansion : Plugin
    {
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
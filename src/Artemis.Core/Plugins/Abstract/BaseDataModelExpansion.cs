using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Artemis.Core.Plugins.Abstract.DataModels;
using Artemis.Core.Plugins.Abstract.DataModels.Attributes;
using Artemis.Core.Utilities;

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

        /// <summary>
        ///     Hide the provided property using a lambda expression, e.g. HideProperty(dm => dm.TimeDataModel.CurrentTimeUTC)
        /// </summary>
        /// <param name="propertyLambda">A lambda expression pointing to the property to ignore</param>
        public void HideProperty<TProperty>(Expression<Func<T, TProperty>> propertyLambda)
        {
            var propertyInfo = ReflectionUtilities.GetPropertyInfo(DataModel, propertyLambda);
            if (!HiddenPropertiesList.Any(p => p.Equals(propertyInfo)))
                HiddenPropertiesList.Add(propertyInfo);
        }

        /// <summary>
        ///     Stop hiding the provided property using a lambda expression, e.g. ShowProperty(dm => dm.TimeDataModel.CurrentTimeUTC)
        /// </summary>
        /// <param name="propertyLambda">A lambda expression pointing to the property to stop ignoring</param>
        public void ShowProperty<TProperty>(Expression<Func<T, TProperty>> propertyLambda)
        {
            var propertyInfo = ReflectionUtilities.GetPropertyInfo(DataModel, propertyLambda);
            HiddenPropertiesList.RemoveAll(p => p.Equals(propertyInfo));
        }

        internal override void InternalEnablePlugin()
        {
            DataModel = Activator.CreateInstance<T>();
            DataModel.PluginInfo = PluginInfo;
            DataModel.DataModelDescription = GetDataModelDescription();
            base.InternalEnablePlugin();
        }

        internal override void InternalDisablePlugin()
        {
            DataModel = null;
            base.InternalDisablePlugin();
        }
    }

    /// <summary>
    ///     For internal use only, to implement your own layer property type, extend <see cref="BaseDataModelExpansion{T}" />
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
using System;
using System.Linq;
using System.Linq.Expressions;

namespace Artemis.Core.DataModelExpansions
{
    /// <summary>
    ///     Allows you to expand the application-wide datamodel
    /// </summary>
    public abstract class DataModelExpansion<T> : BaseDataModelExpansion where T : DataModel
    {
        /// <summary>
        ///     The main data model of this data model expansion
        ///     <para>Note: This default data model is automatically registered upon plugin enable</para>
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
        ///     Stop hiding the provided property using a lambda expression, e.g. ShowProperty(dm =>
        ///     dm.TimeDataModel.CurrentTimeUTC)
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
}
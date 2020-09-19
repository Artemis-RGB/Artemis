using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Artemis.Core.Modules;

namespace Artemis.Core.DataModelExpansions
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

        /// <summary>
        ///     Gets the is expansion status indicating whether this data model expands the main data model
        /// </summary>
        [DataModelIgnore]
        public bool IsExpansion { get; internal set; }

        internal bool ContainsPath(string path)
        {
            var parts = path.Split('.');
            var current = GetType();
            foreach (var part in parts)
            {
                var property = current?.GetProperty(part);
                current = property?.PropertyType;
                if (property == null)
                    return false;
            }

            return true;
        }

        internal Type GetTypeAtPath(string path)
        {
            if (!ContainsPath(path))
                return null;

            var parts = path.Split('.');
            var current = GetType();

            Type result = null;
            foreach (var part in parts)
            {
                var property = current.GetProperty(part);
                current = property.PropertyType;
                result = property.PropertyType;
            }

            return result;
        }

        internal Type GetListTypeInPath(string path)
        {
            if (!ContainsPath(path))
                return null;

            var parts = path.Split('.');
            var current = GetType();

            var index = 0;
            foreach (var part in parts)
            {
                // Only return a type if the path CONTAINS a list, not if it points TO a list
                if (index == parts.Length - 1)
                    return null;

                var property = current.GetProperty(part);

                // For lists, look into the list type instead of the list itself
                if (typeof(IList).IsAssignableFrom(property.PropertyType))
                    return property.PropertyType.GetGenericArguments()[0];

                current = property.PropertyType;
                index++;
            }

            return null;
        }

        internal Type GetListTypeAtPath(string path)
        {
            if (!ContainsPath(path))
                return null;

            var child = GetTypeAtPath(path);
            return child.GenericTypeArguments.Length > 0 ? child.GenericTypeArguments[0] : null;
        }
        
        /// <summary>
        ///     Returns a read-only list of all properties in this datamodel that are to be ignored
        /// </summary>
        /// <returns></returns>
        public ReadOnlyCollection<PropertyInfo> GetHiddenProperties()
        {
            if (PluginInfo.Instance is ProfileModule profileModule)
                return profileModule.HiddenProperties;
            if (PluginInfo.Instance is BaseDataModelExpansion dataModelExpansion)
                return dataModelExpansion.HiddenProperties;

            return new List<PropertyInfo>().AsReadOnly();
        }
    }
}
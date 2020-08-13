using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using Artemis.Core.Exceptions;
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

        public bool ContainsPath(string path)
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

        public Type GetTypeAtPath(string path)
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

        public Type GetListTypeInPath(string path)
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

        public Type GetListTypeAtPath(string path)
        {
            if (!ContainsPath(path))
                return null;

            var child = GetTypeAtPath(path);
            return child.GenericTypeArguments.Length > 0 ? child.GenericTypeArguments[0] : null;
        }

        public string GetListInnerPath(string path)
        {
            if (GetListTypeInPath(path) == null)
                throw new ArtemisCoreException($"Cannot determine inner list path at {path} because it does not contain a list");

            var parts = path.Split('.');
            var current = GetType();

            for (var index = 0; index < parts.Length; index++)
            {
                var part = parts[index];
                var property = current.GetProperty(part);

                if (typeof(IList).IsAssignableFrom(property.PropertyType))
                    return string.Join('.', parts.Skip(index + 1).ToList());

                current = property.PropertyType;
            }

            return null;
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
using System;
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
    }
}
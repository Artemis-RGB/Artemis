using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.Core.Exceptions;
using Artemis.Core.Plugins.Abstract.DataModels.Attributes;
using Artemis.Core.Plugins.Exceptions;
using Artemis.Core.Plugins.Models;
using SkiaSharp;

namespace Artemis.Core.Plugins.Abstract.DataModels
{
    public abstract class DataModel
    {
        /// <summary>
        ///     Gets the plugin info this data model belongs to
        /// </summary>
        public PluginInfo PluginInfo { get; internal set; }

        /// <summary>
        ///     Gets whether this data model is initialized
        /// </summary>
        public bool Initialized { get; private set; }

        /// <summary>
        ///     Gets the <see cref="DataModelPropertyAttribute" /> describing this data model
        /// </summary>
        public DataModelPropertyAttribute DataModelDescription { get; internal set; }

        /// <summary>
        ///     If found on this type, returns the <see cref="DataModelPropertyAttribute" /> for the provided property name
        /// </summary>
        /// <param name="propertyName">The name of the property on to look for</param>
        public DataModelPropertyAttribute GetPropertyAttribute(string propertyName)
        {
            var propertyInfo = GetType().GetProperty(propertyName);
            if (propertyInfo == null)
                return null;

            return (DataModelPropertyAttribute) Attribute.GetCustomAttribute(propertyInfo, typeof(DataModelPropertyAttribute));
        }

        internal void Initialize()
        {
            // Doubt this will happen but let's make sure
            if (Initialized)
                throw new ArtemisCoreException("Data model already initialized, wut");

            foreach (var propertyInfo in GetType().GetProperties())
            {
                var dataModelPropertyAttribute = (DataModelPropertyAttribute) Attribute.GetCustomAttribute(propertyInfo, typeof(DataModelPropertyAttribute));
                if (dataModelPropertyAttribute == null || !typeof(DataModel).IsAssignableFrom(propertyInfo.PropertyType))
                    continue;

                // If the property is a nested datamodel create an instance and initialize it
                var instance = (DataModel) Activator.CreateInstance(propertyInfo.PropertyType, true);
                if (instance == null)
                    throw new ArtemisCoreException($"Failed to create instance of child datamodel at {propertyInfo.Name}");

                instance.PluginInfo = PluginInfo;
                instance.DataModelDescription = dataModelPropertyAttribute;
                instance.Initialize();

                propertyInfo.SetValue(this, instance);
            }

            Initialized = true;
        }
    }
}
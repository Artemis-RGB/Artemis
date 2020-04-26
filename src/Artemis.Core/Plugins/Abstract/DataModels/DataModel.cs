using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.Core.Plugins.Abstract.DataModels.Attributes;
using Artemis.Core.Plugins.Exceptions;
using SkiaSharp;

namespace Artemis.Core.Plugins.Abstract.DataModels
{
    public abstract class DataModel
    {
        private static readonly List<Type> SupportedTypes = new List<Type>
        {
            typeof(bool),
            typeof(byte),
            typeof(decimal),
            typeof(double),
            typeof(float),
            typeof(int),
            typeof(long),
            typeof(string),
            typeof(SKColor),
            typeof(SKPoint)
        };

        protected DataModel(Module module)
        {
            Module = module;
            Validate();
        }

        public Module Module { get; }

        /// <summary>
        ///     Recursively validates the current datamodel, ensuring all properties annotated with
        ///     <see cref="DataModelPropertyAttribute" /> are of supported types.
        /// </summary>
        /// <returns></returns>
        public bool Validate()
        {
            return ValidateType(GetType());
        }

        private bool ValidateType(Type type)
        {
            foreach (var propertyInfo in type.GetProperties())
            {
                var dataModelPropertyAttribute = (DataModelPropertyAttribute) Attribute.GetCustomAttribute(propertyInfo, typeof(DataModelPropertyAttribute));
                if (dataModelPropertyAttribute == null)
                    continue;

                // If the a nested datamodel, ensure the properties on there are valid
                if (propertyInfo.PropertyType == typeof(DataModel))
                    ValidateType(propertyInfo.PropertyType);
                else if (!SupportedTypes.Contains(propertyInfo.PropertyType))
                {
                    // Show a useful error for plugin devs
                    throw new ArtemisPluginException(Module.PluginInfo,
                        $"Plugin datamodel contains property of unsupported type {propertyInfo.PropertyType.Name}. \r\n\r\n" +
                        $"Property name: {propertyInfo.Name}\r\n" +
                        $"Property declared on: {propertyInfo.DeclaringType?.Name ?? "-"} \r\n\r\n" +
                        $"Supported properties:\r\n{string.Join("\r\n", SupportedTypes.Select(t => $" - {t.Name}"))}");
                }
            }

            return true;
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Artemis.Core.Modules;
using Humanizer;

namespace Artemis.Core.DataModelExpansions
{
    public abstract class DataModel
    {
        private readonly Dictionary<string, DataModel> _dynamicDataModels = new Dictionary<string, DataModel>();

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

        /// <summary>
        ///     Gets an read-only dictionary of all dynamic data models
        /// </summary>
        [DataModelIgnore]
        public ReadOnlyDictionary<string, DataModel> DynamicDataModels => new ReadOnlyDictionary<string, DataModel>(_dynamicDataModels);

        /// <summary>
        ///     Returns a read-only collection of all properties in this datamodel that are to be ignored
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

        /// <summary>
        ///     Adds a dynamic data model to this data model
        /// </summary>
        /// <param name="dynamicDataModel">The dynamic data model to add</param>
        /// <param name="key">The key of the child, must be unique to this data model</param>
        /// <param name="name">An optional name, if not provided the key will be used in a humanized form</param>
        /// <param name="description">An optional description</param>
        public void AddDynamicChild(DataModel dynamicDataModel, string key, string name = null, string description = null)
        {
            if (dynamicDataModel == null)
                throw new ArgumentNullException(nameof(dynamicDataModel));
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            if (_dynamicDataModels.ContainsKey(key))
            {
                throw new ArtemisCoreException($"Cannot add a dynamic data model with key '{key}' " +
                                               "because the key is already in use on this data model.");
            }

            if (_dynamicDataModels.ContainsValue(dynamicDataModel))
            {
                var existingKey = _dynamicDataModels.First(kvp => kvp.Value == dynamicDataModel).Key;
                throw new ArtemisCoreException($"Cannot add a dynamic data model with key '{key}' " +
                                               $"because the dynamic data model is already added with key '{existingKey}.");
            }

            dynamicDataModel.PluginInfo = PluginInfo;
            dynamicDataModel.DataModelDescription = new DataModelPropertyAttribute
            {
                Name = name ?? key.Humanize(),
                Description = description
            };
            _dynamicDataModels.Add(key, dynamicDataModel);
        }

        /// <summary>
        ///     Removes a dynamic data model from the data model by its key
        /// </summary>
        /// <param name="key">The key of the dynamic data model to remove</param>
        public void RemoveDynamicChildByKey(string key)
        {
            _dynamicDataModels.Remove(key);
        }

        /// <summary>
        ///     Removes a dynamic data model from this data model
        /// </summary>
        /// <param name="dynamicDataModel">The dynamic data model to remove</param>
        public void RemoveDynamicChild(DataModel dynamicDataModel)
        {
            var keys = _dynamicDataModels.Where(kvp => kvp.Value == dynamicDataModel).Select(kvp => kvp.Key).ToList();
            foreach (var key in keys)
                _dynamicDataModels.Remove(key);
        }

        /// <summary>
        ///     Gets a dynamic data model of type <typeparamref name="T" /> by its key
        /// </summary>
        /// <typeparam name="T">The type of data model you expect</typeparam>
        /// <param name="key">The unique key of the dynamic data model</param>
        /// <returns>If found, the dynamic data model otherwise <c>null</c></returns>
        public T DynamicChild<T>(string key) where T : DataModel
        {
            _dynamicDataModels.TryGetValue(key, out var value);
            return value as T;
        }

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

        #region Events

        /// <summary>
        ///     Occurs when a dynamic data model has been added to this data model
        /// </summary>
        public event EventHandler DynamicDataBindingAdded;

        /// <summary>
        ///     Occurs when a dynamic data model has been removed from this data model
        /// </summary>
        public event EventHandler DynamicDataBindingRemoved;

        protected virtual void OnDynamicDataBindingAdded()
        {
            DynamicDataBindingAdded?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnDynamicDataBindingRemoved()
        {
            DynamicDataBindingRemoved?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}
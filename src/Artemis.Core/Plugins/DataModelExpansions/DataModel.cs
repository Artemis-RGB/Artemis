using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Artemis.Core.Modules;
using Humanizer;
using Newtonsoft.Json;

namespace Artemis.Core.DataModelExpansions
{
    /// <summary>
    ///     Represents a data model that contains information on a game/application etc.
    /// </summary>
    public abstract class DataModel
    {
        private readonly Dictionary<string, DynamicChild> _dynamicChildren = new();

        /// <summary>
        ///     Creates a new instance of the <see cref="DataModel" /> class
        /// </summary>
        protected DataModel()
        {
            // These are both set right after construction to keep the constructor of inherited classes clean
            Feature = null!;
            DataModelDescription = null!;
        }

        /// <summary>
        ///     Gets the plugin feature this data model belongs to
        /// </summary>
        [JsonIgnore]
        [DataModelIgnore]
        public DataModelPluginFeature Feature { get; internal set; }

        /// <summary>
        ///     Gets the <see cref="DataModelPropertyAttribute" /> describing this data model
        /// </summary>
        [JsonIgnore]
        [DataModelIgnore]
        public DataModelPropertyAttribute DataModelDescription { get; internal set; }

        /// <summary>
        ///     Gets the is expansion status indicating whether this data model expands the main data model
        /// </summary>
        [DataModelIgnore]
        public bool IsExpansion { get; internal set; }

        /// <summary>
        ///     Gets an read-only dictionary of all dynamic children
        /// </summary>
        [DataModelIgnore]
        public ReadOnlyDictionary<string, DynamicChild> DynamicChildren => new(_dynamicChildren);

        /// <summary>
        ///     Returns a read-only collection of all properties in this datamodel that are to be ignored
        /// </summary>
        /// <returns></returns>
        public ReadOnlyCollection<PropertyInfo> GetHiddenProperties()
        {
            if (Feature is ProfileModule profileModule)
                return profileModule.HiddenProperties;
            if (Feature is BaseDataModelExpansion dataModelExpansion)
                return dataModelExpansion.HiddenProperties;

            return new List<PropertyInfo>().AsReadOnly();
        }

        /// <summary>
        ///     Adds a dynamic child to this data model
        /// </summary>
        /// <param name="dynamicChild">The dynamic child to add</param>
        /// <param name="key">The key of the child, must be unique to this data model</param>
        /// <param name="name">An optional human readable name, if not provided the key will be used in a humanized form</param>
        public T AddDynamicChild<T>(T dynamicChild, string key, string? name = null)
        {
            if (dynamicChild == null)
                throw new ArgumentNullException(nameof(dynamicChild));
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            if (key.Contains('.'))
                throw new ArtemisCoreException("The provided key contains an illegal character (.)");
            if (_dynamicChildren.ContainsKey(key))
            {
                throw new ArtemisCoreException($"Cannot add a dynamic child with key '{key}' " +
                                               "because the key is already in use on by another dynamic property this data model.");
            }

            if (GetType().GetProperty(key) != null)
            {
                throw new ArtemisCoreException($"Cannot add a dynamic child with key '{key}' " +
                                               "because the key is already in use by a static property on this data model.");
            }

            DataModelPropertyAttribute attribute = new()
            {
                Name = string.IsNullOrWhiteSpace(name) ? key.Humanize() : name
            };
            if (dynamicChild is DataModel dynamicDataModel)
            {
                dynamicDataModel.Feature = Feature;
                dynamicDataModel.DataModelDescription = attribute;
            }

            _dynamicChildren.Add(key, new DynamicChild(attribute, dynamicChild));

            OnDynamicDataModelAdded(new DynamicDataModelChildEventArgs(dynamicChild, key));
            return dynamicChild;
        }

        /// <summary>
        ///     Adds a dynamic child to this data model
        /// </summary>
        /// <param name="dynamicChild">The dynamic child to add</param>
        /// <param name="key">The key of the child, must be unique to this data model</param>
        /// <param name="name">A human readable for your dynamic child, shown in the UI</param>
        /// <param name="description">An optional description, shown in the UI</param>
        public T AddDynamicChild<T>(T dynamicChild, string key, string name, string description)
        {
            if (dynamicChild == null)
                throw new ArgumentNullException(nameof(dynamicChild));
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            if (key.Contains('.'))
                throw new ArtemisCoreException("The provided key contains an illegal character (.)");
            if (_dynamicChildren.ContainsKey(key))
            {
                throw new ArtemisCoreException($"Cannot add a dynamic child with key '{key}' " +
                                               "because the key is already in use on by another dynamic property this data model.");
            }

            if (GetType().GetProperty(key) != null)
            {
                throw new ArtemisCoreException($"Cannot add a dynamic child with key '{key}' " +
                                               "because the key is already in use by a static property on this data model.");
            }

            DataModelPropertyAttribute attribute = new()
            {
                Name = string.IsNullOrWhiteSpace(name) ? key.Humanize() : name,
                Description = description
            };
            if (dynamicChild is DataModel dynamicDataModel)
            {
                dynamicDataModel.Feature = Feature;
                dynamicDataModel.DataModelDescription = attribute;
            }

            _dynamicChildren.Add(key, new DynamicChild(attribute, dynamicChild));

            OnDynamicDataModelAdded(new DynamicDataModelChildEventArgs(dynamicChild, key));
            return dynamicChild;
        }

        /// <summary>
        ///     Adds a dynamic child to this data model
        /// </summary>
        /// <param name="dynamicChild">The dynamic child to add</param>
        /// <param name="key">The key of the child, must be unique to this data model</param>
        /// <param name="attribute">A data model property attribute describing the dynamic child</param>
        public T AddDynamicChild<T>(T dynamicChild, string key, DataModelPropertyAttribute attribute)
        {
            if (dynamicChild == null) throw new ArgumentNullException(nameof(dynamicChild));
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (attribute == null) throw new ArgumentNullException(nameof(attribute));
            if (key.Contains('.'))
                throw new ArtemisCoreException("The provided key contains an illegal character (.)");
            if (_dynamicChildren.ContainsKey(key))
            {
                throw new ArtemisCoreException($"Cannot add a dynamic child with key '{key}' " +
                                               "because the key is already in use on by another dynamic property this data model.");
            }

            if (GetType().GetProperty(key) != null)
            {
                throw new ArtemisCoreException($"Cannot add a dynamic child with key '{key}' " +
                                               "because the key is already in use by a static property on this data model.");
            }

            // Make sure a name is on the attribute or funny things might happen
            attribute.Name ??= key.Humanize();
            if (dynamicChild is DataModel dynamicDataModel)
            {
                dynamicDataModel.Feature = Feature;
                dynamicDataModel.DataModelDescription = attribute;
            }

            _dynamicChildren.Add(key, new DynamicChild(attribute, dynamicChild));

            OnDynamicDataModelAdded(new DynamicDataModelChildEventArgs(dynamicChild, key));
            return dynamicChild;
        }

        /// <summary>
        ///     Removes a dynamic child from the data model by its key
        /// </summary>
        /// <param name="key">The key of the dynamic child to remove</param>
        public void RemoveDynamicChildByKey(string key)
        {
            if (!_dynamicChildren.TryGetValue(key, out DynamicChild? dynamicChild))
                return;

            _dynamicChildren.Remove(key);
            OnDynamicDataModelRemoved(new DynamicDataModelChildEventArgs(dynamicChild.Value, key));
        }

        /// <summary>
        ///     Removes a dynamic child from this data model
        /// </summary>
        /// <param name="dynamicChild">The dynamic data child to remove</param>
        public void RemoveDynamicChild<T>(T dynamicChild) where T : class
        {
            List<string> keys = _dynamicChildren.Where(kvp => kvp.Value.Value == dynamicChild).Select(kvp => kvp.Key).ToList();
            foreach (string key in keys)
            {
                _dynamicChildren.Remove(key);
                OnDynamicDataModelRemoved(new DynamicDataModelChildEventArgs(dynamicChild, key));
            }
        }

        /// <summary>
        ///     Removes all dynamic children from this data model
        /// </summary>
        public void ClearDynamicChildren()
        {
            while (_dynamicChildren.Any())
                RemoveDynamicChildByKey(_dynamicChildren.First().Key);
        }

        /// <summary>
        ///     Gets a dynamic child of type <typeparamref name="T" /> by its key
        /// </summary>
        /// <typeparam name="T">The type of data model you expect</typeparam>
        /// <param name="key">The unique key of the dynamic child</param>
        /// <returns>If found, the dynamic child otherwise <c>null</c></returns>
        public T? DynamicChild<T>(string key)
        {
            if (!_dynamicChildren.TryGetValue(key, out DynamicChild? dynamicChild))
                return default;

            if (dynamicChild.Value is not T)
                return default;
            return (T?) dynamicChild.Value;
        }

        /// <summary>
        ///     Occurs when a dynamic child has been added to this data model
        /// </summary>
        public event EventHandler<DynamicDataModelChildEventArgs>? DynamicChildAdded;

        /// <summary>
        ///     Occurs when a dynamic child has been removed from this data model
        /// </summary>
        public event EventHandler<DynamicDataModelChildEventArgs>? DynamicChildRemoved;

        /// <summary>
        ///     Invokes the <see cref="DynamicChildAdded" /> event
        /// </summary>
        protected virtual void OnDynamicDataModelAdded(DynamicDataModelChildEventArgs e)
        {
            DynamicChildAdded?.Invoke(this, e);
        }

        /// <summary>
        ///     Invokes the <see cref="DynamicChildRemoved" /> event
        /// </summary>
        protected virtual void OnDynamicDataModelRemoved(DynamicDataModelChildEventArgs e)
        {
            DynamicChildRemoved?.Invoke(this, e);
        }
    }

    /// <summary>
    ///     Represents a record of a dynamic child value with its property attribute
    /// </summary>
    public record DynamicChild(DataModelPropertyAttribute Attribute, object? Value);
}
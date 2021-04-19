using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
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

        #region Dynamic children

        /// <summary>
        ///     Adds a dynamic child to this data model
        /// </summary>
        /// <param name="key">The key of the child, must be unique to this data model</param>
        /// <param name="initialValue">The initial value of the dynamic child</param>
        /// <returns>The resulting dynamic child which can be used to further update the value</returns>
        public DynamicChild<T> AddDynamicChild<T>(string key, T initialValue)
        {
            return AddDynamicChild(key, initialValue, new DataModelPropertyAttribute());
        }

        /// <summary>
        ///     Adds a dynamic child to this data model
        /// </summary>
        /// <param name="key">The key of the child, must be unique to this data model</param>
        /// <param name="initialValue">The initial value of the dynamic child</param>
        /// <param name="name">A human readable name for your dynamic child, shown in the UI</param>
        /// <param name="description">An optional description, shown in the UI</param>
        /// <returns>The resulting dynamic child which can be used to further update the value</returns>
        public DynamicChild<T> AddDynamicChild<T>(string key, T initialValue, string name, string? description = null)
        {
            return AddDynamicChild(key, initialValue, new DataModelPropertyAttribute {Name = name, Description = description});
        }

        /// <summary>
        ///     Adds a dynamic child to this data model
        /// </summary>
        /// <param name="key">The key of the child, must be unique to this data model</param>
        /// <param name="initialValue">The initial value of the dynamic child</param>
        /// <param name="attribute">A data model property attribute describing the dynamic child</param>
        /// <returns>The resulting dynamic child which can be used to further update the value</returns>
        public DynamicChild<T> AddDynamicChild<T>(string key, T initialValue, DataModelPropertyAttribute attribute)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (initialValue == null) throw new ArgumentNullException(nameof(initialValue));
            if (attribute == null) throw new ArgumentNullException(nameof(attribute));
            if (key.Contains('.'))
                throw new ArtemisCoreException("The provided key contains an illegal character (.)");
            if (_dynamicChildren.ContainsKey(key))
                throw new ArtemisCoreException($"Cannot add a dynamic child with key '{key}' " +
                                               "because the key is already in use on by another dynamic property this data model.");

            if (GetType().GetProperty(key) != null)
                throw new ArtemisCoreException($"Cannot add a dynamic child with key '{key}' " +
                                               "because the key is already in use by a static property on this data model.");

            // Make sure a name is on the attribute or funny things might happen
            attribute.Name ??= key.Humanize();
            if (initialValue is DataModel dynamicDataModel)
            {
                dynamicDataModel.Feature = Feature;
                dynamicDataModel.DataModelDescription = attribute;
            }

            DynamicChild<T> dynamicChild = new(initialValue, key, attribute);
            _dynamicChildren.Add(key, dynamicChild);

            OnDynamicDataModelAdded(new DynamicDataModelChildEventArgs(dynamicChild, key));
            return dynamicChild;
        }
        
        /// <summary>
        ///     Gets a previously added dynamic child by its key
        /// </summary>
        /// <param name="key">The key of the dynamic child</param>
        public DynamicChild GetDynamicChild(string key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            return DynamicChildren[key];
        }

        /// <summary>
        ///     Gets a previously added dynamic child by its key
        /// </summary>
        /// <typeparam name="T">The typer of dynamic child you are expecting</typeparam>
        /// <param name="key">The key of the dynamic child</param>
        /// <returns></returns>
        public DynamicChild<T> GetDynamicChild<T>(string key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            return (DynamicChild<T>) DynamicChildren[key];
        }

        /// <summary>
        ///     Gets a previously added dynamic child by its key
        /// </summary>
        /// <param name="key">The key of the dynamic child</param>
        /// <param name="dynamicChild">
        ///     When this method returns, the <see cref="DynamicChild" /> associated with the specified key,
        ///     if the key is found; otherwise, <see langword="null" />. This parameter is passed uninitialized.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if the data model contains the dynamic child; otherwise <see langword="false" />
        /// </returns>
        public bool TryGetDynamicChild(string key, [MaybeNullWhen(false)] out DynamicChild dynamicChild)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            dynamicChild = null;
            if (!DynamicChildren.TryGetValue(key, out DynamicChild? value))
                return false;

            dynamicChild = value;
            return true;
        }

        /// <summary>
        ///     Gets a previously added dynamic child by its key
        /// </summary>
        /// <typeparam name="T">The typer of dynamic child you are expecting</typeparam>
        /// <param name="key">The key of the dynamic child</param>
        /// <param name="dynamicChild">
        ///     When this method returns, the <see cref="DynamicChild{T}" /> associated with the specified
        ///     key, if the key is found; otherwise, <see langword="null" />. This parameter is passed uninitialized.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if the data model contains the dynamic child; otherwise <see langword="false" />
        /// </returns>
        public bool TryGetDynamicChild<T>(string key, [MaybeNullWhen(false)] out DynamicChild<T> dynamicChild)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            dynamicChild = null;
            if (!DynamicChildren.TryGetValue(key, out DynamicChild? value))
                return false;
            if (value is not DynamicChild<T> typedDynamicChild)
                return false;
            dynamicChild = typedDynamicChild;
            return true;
        }

        /// <summary>
        ///     Removes a dynamic child from the data model by its key
        /// </summary>
        /// <param name="key">The key of the dynamic child to remove</param>
        public void RemoveDynamicChildByKey(string key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (!_dynamicChildren.TryGetValue(key, out DynamicChild? dynamicChild))
                return;

            _dynamicChildren.Remove(key);
            OnDynamicDataModelRemoved(new DynamicDataModelChildEventArgs(dynamicChild, key));
        }

        /// <summary>
        ///     Removes a dynamic child from this data model
        /// </summary>
        /// <param name="dynamicChild">The dynamic data child to remove</param>
        public void RemoveDynamicChild(DynamicChild dynamicChild)
        {
            if (dynamicChild == null) throw new ArgumentNullException(nameof(dynamicChild));
            List<string> keys = _dynamicChildren.Where(kvp => kvp.Value.BaseValue == dynamicChild).Select(kvp => kvp.Key).ToList();
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

        // Used a runtime by data model paths only
        internal T? GetDynamicChildValue<T>(string key)
        {
            return TryGetDynamicChild(key, out DynamicChild<T>? dynamicChild) ? dynamicChild.Value : default;
        }

        #endregion
    }
}
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Artemis.Core.Annotations;
using Artemis.Core.Events;
using Artemis.Core.Exceptions;
using Artemis.Core.Models.Profile.LayerProperties;
using Artemis.Core.Models.Profile.LayerProperties.Attributes;
using Artemis.Core.Plugins.Exceptions;
using Artemis.Core.Plugins.LayerBrush.Abstract;
using Artemis.Core.Plugins.LayerEffect.Abstract;
using Artemis.Core.Services.Interfaces;
using Artemis.Storage.Entities.Profile;

namespace Artemis.Core.Models.Profile
{
    public abstract class LayerPropertyGroup : IDisposable
    {
        private readonly List<BaseLayerProperty> _layerProperties;
        private readonly List<LayerPropertyGroup> _layerPropertyGroups;
        private ReadOnlyCollection<BaseLayerProperty> _allLayerProperties;
        private bool _isHidden;

        protected LayerPropertyGroup()
        {
            _layerProperties = new List<BaseLayerProperty>();
            _layerPropertyGroups = new List<LayerPropertyGroup>();
        }

        /// <summary>
        ///     Gets the profile element (such as layer or folder) this effect is applied to
        /// </summary>
        public RenderProfileElement ProfileElement { get; internal set; }

        /// <summary>
        ///     The path of this property group
        /// </summary>
        public string Path { get; internal set; }

        /// <summary>
        ///     The parent group of this layer property group, set after construction
        /// </summary>
        public LayerPropertyGroup Parent { get; internal set; }

        /// <summary>
        ///     Gets whether this property groups properties are all initialized
        /// </summary>
        public bool PropertiesInitialized { get; private set; }

        /// <summary>
        ///     Used to declare that this property group doesn't belong to a plugin and should use the core plugin GUID
        /// </summary>
        public bool IsCorePropertyGroup { get; internal set; }

        public PropertyGroupDescriptionAttribute GroupDescription { get; internal set; }

        /// <summary>
        ///     The layer brush this property group belongs to
        /// </summary>
        public BaseLayerBrush LayerBrush { get; internal set; }

        /// <summary>
        ///     The layer effect this property group belongs to
        /// </summary>
        public BaseLayerEffect LayerEffect { get; internal set; }

        /// <summary>
        ///     Gets or sets whether the property is hidden in the UI
        /// </summary>
        public bool IsHidden
        {
            get => _isHidden;
            set
            {
                _isHidden = value;
                OnVisibilityChanged();
            }
        }

        /// <summary>
        ///     A list of all layer properties in this group
        /// </summary>
        public ReadOnlyCollection<BaseLayerProperty> LayerProperties => _layerProperties.AsReadOnly();

        /// <summary>
        ///     A list of al child groups in this group
        /// </summary>
        public ReadOnlyCollection<LayerPropertyGroup> LayerPropertyGroups => _layerPropertyGroups.AsReadOnly();

        /// <summary>
        ///     Recursively gets all layer properties on this group and any subgroups
        /// </summary>
        /// <returns></returns>
        public IReadOnlyCollection<BaseLayerProperty> GetAllLayerProperties()
        {
            if (!PropertiesInitialized)
                return new List<BaseLayerProperty>();
            if (_allLayerProperties != null)
                return _allLayerProperties;

            var result = new List<BaseLayerProperty>(LayerProperties);
            foreach (var layerPropertyGroup in LayerPropertyGroups)
                result.AddRange(layerPropertyGroup.GetAllLayerProperties());

            _allLayerProperties = result.AsReadOnly();
            return _allLayerProperties;
        }

        public void Dispose()
        {
            DisableProperties();
            foreach (var layerPropertyGroup in _layerPropertyGroups)
                layerPropertyGroup.Dispose();
        }

        /// <summary>
        ///     Called before property group is activated to allow you to populate <see cref="LayerProperty{T}.DefaultValue" /> on
        ///     the properties you want
        /// </summary>
        protected abstract void PopulateDefaults();

        /// <summary>
        ///     Called when the property group is deactivated
        /// </summary>
        protected abstract void EnableProperties();

        /// <summary>
        ///     Called when the property group is deactivated (either the profile unloaded or the related brush/effect was removed)
        /// </summary>
        protected abstract void DisableProperties();

        protected virtual void OnPropertyGroupInitialized()
        {
            PropertyGroupInitialized?.Invoke(this, EventArgs.Empty);
        }

        internal void InitializeProperties(IRenderElementService renderElementService, RenderProfileElement profileElement, [NotNull] string path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));
            // Doubt this will happen but let's make sure
            if (PropertiesInitialized)
                throw new ArtemisCoreException("Layer property group already initialized, wut");

            ProfileElement = profileElement;
            Path = path.TrimEnd('.');

            // Get all properties with a PropertyDescriptionAttribute
            foreach (var propertyInfo in GetType().GetProperties())
            {
                var propertyDescription = Attribute.GetCustomAttribute(propertyInfo, typeof(PropertyDescriptionAttribute));
                if (propertyDescription != null)
                {
                    if (!typeof(BaseLayerProperty).IsAssignableFrom(propertyInfo.PropertyType))
                        throw new ArtemisPluginException($"Layer property with PropertyDescription attribute must be of type LayerProperty at {path + propertyInfo.Name}");

                    var instance = (BaseLayerProperty) Activator.CreateInstance(propertyInfo.PropertyType, true);
                    if (instance == null)
                        throw new ArtemisPluginException($"Failed to create instance of layer property at {path + propertyInfo.Name}");

                    instance.ProfileElement = profileElement;
                    instance.Parent = this;
                    instance.PropertyDescription = (PropertyDescriptionAttribute) propertyDescription;
                    if (instance.PropertyDescription.DisableKeyframes)
                        instance.KeyframesSupported = false;

                    InitializeProperty(profileElement, path + propertyInfo.Name, instance);

                    propertyInfo.SetValue(this, instance);
                    _layerProperties.Add(instance);
                }
                else
                {
                    var propertyGroupDescription = Attribute.GetCustomAttribute(propertyInfo, typeof(PropertyGroupDescriptionAttribute));
                    if (propertyGroupDescription != null)
                    {
                        if (!typeof(LayerPropertyGroup).IsAssignableFrom(propertyInfo.PropertyType))
                            throw new ArtemisPluginException("Layer property with PropertyGroupDescription attribute must be of type LayerPropertyGroup");

                        var instance = (LayerPropertyGroup) Activator.CreateInstance(propertyInfo.PropertyType);
                        if (instance == null)
                            throw new ArtemisPluginException($"Failed to create instance of layer property group at {path + propertyInfo.Name}");

                        instance.Parent = this;
                        instance.GroupDescription = (PropertyGroupDescriptionAttribute) propertyGroupDescription;
                        instance.LayerBrush = LayerBrush;
                        instance.LayerEffect = LayerEffect;
                        instance.InitializeProperties(renderElementService, profileElement, $"{path}{propertyInfo.Name}.");

                        propertyInfo.SetValue(this, instance);
                        _layerPropertyGroups.Add(instance);
                    }
                }
            }

            // Request the property group to populate defaults
            PopulateDefaults();

            // Apply the newly populated defaults
            foreach (var layerProperty in _layerProperties.Where(p => !p.IsLoadedFromStorage))
                layerProperty.ApplyDefaultValue();

            EnableProperties();
            PropertiesInitialized = true;
            OnPropertyGroupInitialized();
        }

        internal void ApplyToEntity()
        {
            if (!PropertiesInitialized)
                return;

            // Get all properties with a PropertyDescriptionAttribute
            foreach (var propertyInfo in GetType().GetProperties())
            {
                var propertyDescription = Attribute.GetCustomAttribute(propertyInfo, typeof(PropertyDescriptionAttribute));
                if (propertyDescription != null)
                {
                    var layerProperty = (BaseLayerProperty) propertyInfo.GetValue(this);
                    layerProperty.ApplyToEntity();
                }
                else
                {
                    var propertyGroupDescription = Attribute.GetCustomAttribute(propertyInfo, typeof(PropertyGroupDescriptionAttribute));
                    if (propertyGroupDescription != null)
                    {
                        var layerPropertyGroup = (LayerPropertyGroup) propertyInfo.GetValue(this);
                        layerPropertyGroup.ApplyToEntity();
                    }
                }
            }
        }

        internal void Update()
        {
            // Since at this point we don't know what properties the group has without using reflection,
            // let properties subscribe to the update event and update themselves
            OnPropertyGroupUpdating();
        }

        private void InitializeProperty(RenderProfileElement profileElement, string path, BaseLayerProperty instance)
        {
            Guid pluginGuid;
            if (IsCorePropertyGroup || instance.IsCoreProperty)
                pluginGuid = Constants.CorePluginInfo.Guid;
            else if (instance.Parent.LayerBrush != null)
                pluginGuid = instance.Parent.LayerBrush.PluginInfo.Guid;
            else
                pluginGuid = instance.Parent.LayerEffect.PluginInfo.Guid;

            var entity = profileElement.RenderElementEntity.PropertyEntities.FirstOrDefault(p => p.PluginGuid == pluginGuid && p.Path == path);
            var fromStorage = true;
            if (entity == null)
            {
                fromStorage = false;
                entity = new PropertyEntity {PluginGuid = pluginGuid, Path = path};
                profileElement.RenderElementEntity.PropertyEntities.Add(entity);
            }

            instance.ApplyToLayerProperty(entity, this, fromStorage);
            instance.BaseValueChanged += InstanceOnBaseValueChanged;
        }

        private void InstanceOnBaseValueChanged(object sender, EventArgs e)
        {
            OnLayerPropertyBaseValueChanged(new LayerPropertyEventArgs((BaseLayerProperty) sender));
        }

        #region Events

        internal event EventHandler PropertyGroupUpdating;

        /// <summary>
        ///     Occurs when the property group has initialized all its children
        /// </summary>
        public event EventHandler PropertyGroupInitialized;

        /// <summary>
        ///     Occurs when one of the base value of one of the layer properties in this group changes
        ///     <para>Note: Will not trigger on properties in child groups</para>
        /// </summary>
        public event EventHandler<LayerPropertyEventArgs> LayerPropertyBaseValueChanged;

        /// <summary>
        ///     Occurs when the <see cref="IsHidden" /> value of the layer property was updated
        /// </summary>
        public event EventHandler VisibilityChanged;

        protected virtual void OnPropertyGroupUpdating()
        {
            PropertyGroupUpdating?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnVisibilityChanged()
        {
            VisibilityChanged?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnLayerPropertyBaseValueChanged(LayerPropertyEventArgs e)
        {
            LayerPropertyBaseValueChanged?.Invoke(this, e);
        }

        #endregion
    }
}
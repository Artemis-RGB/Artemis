using System;
using Artemis.Storage.Entities.Profile.DataBindings;
using Newtonsoft.Json;

namespace Artemis.Core
{
    /// <inheritdoc />
    public class DataBindingModifier<TLayerProperty, TProperty> : IDataBindingModifier
    {
        private bool _disposed;

        internal DataBindingModifier(DirectDataBinding<TLayerProperty, TProperty> directDataBinding, ProfileRightSideType parameterType)
        {
            DirectDataBinding = directDataBinding ?? throw new ArgumentNullException(nameof(directDataBinding));
            Order = directDataBinding.Modifiers.Count + 1;
            ParameterType = parameterType;
            Entity = new DataBindingModifierEntity();
            Initialize();
            Save();
        }

        internal DataBindingModifier(DirectDataBinding<TLayerProperty, TProperty> directDataBinding, DataBindingModifierEntity entity)
        {
            DirectDataBinding = directDataBinding ?? throw new ArgumentNullException(nameof(directDataBinding));
            Entity = entity;
            Load();
            Initialize();
        }

        /// <summary>
        ///     Gets the type of modifier that is being applied
        /// </summary>
        public BaseDataBindingModifierType? ModifierType { get; private set; }

        /// <summary>
        ///     Gets the direct data binding this modifier is applied to
        /// </summary>
        public DirectDataBinding<TLayerProperty, TProperty> DirectDataBinding { get; }

        /// <summary>
        ///     Gets the type of the parameter, can either be dynamic (based on a data model value) or static
        /// </summary>
        public ProfileRightSideType ParameterType { get; set; }

        /// <summary>
        ///     Gets or sets the position at which the modifier appears on the data binding
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        ///     Gets the path of the parameter property
        /// </summary>
        public DataModelPath? ParameterPath { get; set; }

        /// <summary>
        ///     Gets the parameter static value, only used it <see cref="ParameterType" /> is
        ///     <see cref="ProfileRightSideType.Static" />
        /// </summary>
        public object? ParameterStaticValue { get; private set; }

        internal DataBindingModifierEntity Entity { get; set; }

        /// <summary>
        ///     Applies the modifier to the provided value
        /// </summary>
        /// <param name="currentValue">The value to apply the modifier to, should be of the same type as the data binding target</param>
        /// <returns>The modified value</returns>
        public object? Apply(object? currentValue)
        {
            if (_disposed)
                throw new ObjectDisposedException("DataBindingModifier");

            if (ModifierType == null)
                return currentValue;

            if (ModifierType.ParameterType == null)
                return ModifierType.InternalApply(currentValue, null);

            if (ParameterType == ProfileRightSideType.Dynamic && ParameterPath != null && ParameterPath.IsValid)
            {
                object? value = ParameterPath.GetValue();
                return ModifierType.InternalApply(currentValue, value);
            }

            if (ParameterType == ProfileRightSideType.Static)
                return ModifierType.InternalApply(currentValue, ParameterStaticValue);

            return currentValue;
        }

        /// <summary>
        ///     Updates the modifier type of the modifier and re-compiles the expression
        /// </summary>
        /// <param name="modifierType"></param>
        public void UpdateModifierType(BaseDataBindingModifierType? modifierType)
        {
            if (_disposed)
                throw new ObjectDisposedException("DataBindingModifier");

            if (modifierType == null)
            {
                ModifierType = null;
                return;
            }

            Type? targetType = DirectDataBinding.DataBinding.GetTargetType();
            if (targetType != null && !modifierType.SupportsType(targetType, ModifierTypePart.Value))
                throw new ArtemisCoreException($"Cannot apply modifier type {modifierType.GetType().Name} to this modifier because " +
                                               $"it does not support this data binding's type {targetType.Name}");

            ModifierType = modifierType;
            ValidateParameter();
        }

        private void ValidateParameter()
        {
            if (ModifierType == null)
                return;

            if (ParameterType == ProfileRightSideType.Dynamic)
            {
                if (ParameterPath == null || !ParameterPath.IsValid)
                    return;

                Type parameterType = ParameterPath.GetPropertyType()!;
                if (!ModifierType.SupportsType(parameterType, ModifierTypePart.Parameter))
                    UpdateParameterDynamic(null);
            }
            else
            {
                if (ParameterStaticValue == null)
                    return;

                if (!ModifierType.SupportsType(ParameterStaticValue.GetType(), ModifierTypePart.Parameter))
                    UpdateParameterStatic(null);
            }
        }

        /// <summary>
        ///     Updates the parameter of the modifier and makes the modifier dynamic
        /// </summary>
        /// <param name="path">The path pointing to the parameter</param>
        public void UpdateParameterDynamic(DataModelPath? path)
        {
            if (_disposed)
                throw new ObjectDisposedException("DataBindingModifier");

            if (path != null && !path.IsValid)
                throw new ArtemisCoreException("Cannot update parameter of data binding modifier to an invalid path");

            ParameterPath?.Dispose();
            ParameterPath = path != null ? new DataModelPath(path) : null;

            ParameterType = ProfileRightSideType.Dynamic;
        }

        /// <summary>
        ///     Updates the parameter of the modifier, makes the modifier static and re-compiles the expression
        /// </summary>
        /// <param name="staticValue">The static value to use as a parameter</param>
        public void UpdateParameterStatic(object? staticValue)
        {
            if (_disposed)
                throw new ObjectDisposedException("DataBindingModifier");

            ParameterType = ProfileRightSideType.Static;
            ParameterPath?.Dispose();
            ParameterPath = null;

            Type? parameterType = ModifierType?.ParameterType ?? DirectDataBinding.DataBinding.GetTargetType();

            // If not null ensure the types match and if not, convert it
            if (parameterType == null || staticValue != null && staticValue.GetType() == parameterType)
                ParameterStaticValue = staticValue;
            else if (staticValue != null)
                ParameterStaticValue = Convert.ChangeType(staticValue, parameterType);
            // If null create a default instance for value types or simply make it null for reference types
            else if (parameterType.IsValueType)
                ParameterStaticValue = Activator.CreateInstance(parameterType);
            else
                ParameterStaticValue = null;
        }

        private void Initialize()
        {
            DataBindingModifierTypeStore.DataBindingModifierAdded += DataBindingModifierTypeStoreOnDataBindingModifierAdded;
            DataBindingModifierTypeStore.DataBindingModifierRemoved += DataBindingModifierTypeStoreOnDataBindingModifierRemoved;

            // Modifier type
            if (Entity.ModifierTypePluginGuid != null && ModifierType == null)
            {
                BaseDataBindingModifierType? modifierType = DataBindingModifierTypeStore.Get(Entity.ModifierTypePluginGuid.Value, Entity.ModifierType)?.DataBindingModifierType;
                if (modifierType != null)
                    UpdateModifierType(modifierType);
            }

            // Dynamic parameter
            if (ParameterType == ProfileRightSideType.Dynamic && Entity.ParameterPath != null)
            {
                ParameterPath = new DataModelPath(null, Entity.ParameterPath);
            }
            // Static parameter
            else if (ParameterType == ProfileRightSideType.Static && Entity.ParameterStaticValue != null)
            {
                // Use the target type so JSON.NET has a better idea what to do
                Type? parameterType = ModifierType?.ParameterType ?? DirectDataBinding.DataBinding.GetTargetType();
                object? staticValue = null;

                try
                {
                    staticValue = parameterType != null 
                        ? JsonConvert.DeserializeObject(Entity.ParameterStaticValue, parameterType, Constants.JsonConvertSettings) 
                        : JsonConvert.DeserializeObject(Entity.ParameterStaticValue, Constants.JsonConvertSettings);
                }
                // If deserialization fails, use the type's default
                catch (JsonSerializationException e)
                {
                    DeserializationLogger.LogModifierDeserializationFailure(GetType().Name, e);
                    if (parameterType != null)
                        staticValue = Activator.CreateInstance(parameterType);
                }

                UpdateParameterStatic(staticValue);
            }
        }

        /// <inheritdoc />
        public void Save()
        {
            if (_disposed)
                throw new ObjectDisposedException("DataBindingModifier");

            // Don't save an invalid state
            if (ParameterPath != null && !ParameterPath.IsValid)
                return;

            if (!DirectDataBinding.Entity.Modifiers.Contains(Entity))
                DirectDataBinding.Entity.Modifiers.Add(Entity);

            // Modifier
            if (ModifierType?.Plugin != null)
            {
                Entity.ModifierType = ModifierType.GetType().Name;
                Entity.ModifierTypePluginGuid = ModifierType.Plugin.Guid;
            }

            // General
            Entity.Order = Order;
            Entity.ParameterType = (int) ParameterType;

            // Parameter
            ParameterPath?.Save();
            Entity.ParameterPath = ParameterPath?.Entity;

            Entity.ParameterStaticValue = JsonConvert.SerializeObject(ParameterStaticValue, Constants.JsonConvertSettings);
        }

        /// <inheritdoc />
        public void Load()
        {
            if (_disposed)
                throw new ObjectDisposedException("DataBindingModifier");

            // Modifier type is done during Initialize

            // General
            Order = Entity.Order;
            ParameterType = (ProfileRightSideType) Entity.ParameterType;

            // Parameter is done during initialize
        }

        #region IDisposable

        /// <summary>
        ///     Releases the unmanaged resources used by the object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">
        ///     <see langword="true" /> to release both managed and unmanaged resources;
        ///     <see langword="false" /> to release only unmanaged resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _disposed = true;

                DataBindingModifierTypeStore.DataBindingModifierAdded -= DataBindingModifierTypeStoreOnDataBindingModifierAdded;
                DataBindingModifierTypeStore.DataBindingModifierRemoved -= DataBindingModifierTypeStoreOnDataBindingModifierRemoved;

                ParameterPath?.Dispose();
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Event handlers

        private void DataBindingModifierTypeStoreOnDataBindingModifierAdded(object? sender, DataBindingModifierTypeStoreEvent e)
        {
            if (ModifierType != null)
                return;

            BaseDataBindingModifierType modifierType = e.TypeRegistration.DataBindingModifierType;
            if (modifierType.Plugin!.Guid == Entity.ModifierTypePluginGuid && modifierType.GetType().Name == Entity.ModifierType)
                UpdateModifierType(modifierType);
        }

        private void DataBindingModifierTypeStoreOnDataBindingModifierRemoved(object? sender, DataBindingModifierTypeStoreEvent e)
        {
            if (e.TypeRegistration.DataBindingModifierType == ModifierType)
                UpdateModifierType(null);
        }

        #endregion
    }
}
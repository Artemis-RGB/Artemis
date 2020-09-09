using System;
using System.Linq;
using System.Linq.Expressions;
using Artemis.Core.DataModelExpansions;
using Artemis.Storage.Entities.Profile.DataBindings;
using Newtonsoft.Json;

namespace Artemis.Core
{
    /// <inheritdoc />
    public class DataBindingModifier<TLayerProperty, TProperty> : IDataBindingModifier
    {
        private DataBinding<TLayerProperty, TProperty> _dataBinding;
        private bool _disposed;

        /// <summary>
        ///     Creates a new instance of the <see cref="DataBindingModifier{TLayerProperty,TProperty}" /> class
        /// </summary>
        /// <param name="parameterType">The type of the parameter, can either be dynamic (based on a data model value) or static</param>
        public DataBindingModifier(ProfileRightSideType parameterType)
        {
            ParameterType = parameterType;
            Entity = new DataBindingModifierEntity();
            Save();
            Initialize();
        }

        internal DataBindingModifier(DataBinding<TLayerProperty, TProperty> dataBinding, DataBindingModifierEntity entity)
        {
            DataBinding = dataBinding;
            Entity = entity;
            Load();
            Initialize();
        }

        /// <summary>
        ///     Gets the data binding this modifier is applied to
        /// </summary>
        public DataBinding<TLayerProperty, TProperty> DataBinding
        {
            get => _dataBinding;
            internal set
            {
                _dataBinding = value;
                CreateExpression();
            }
        }

        /// <summary>
        ///     Gets the type of modifier that is being applied
        /// </summary>
        public DataBindingModifierType ModifierType { get; private set; }

        /// <summary>
        ///     Gets the type of the parameter, can either be dynamic (based on a data model value) or static
        /// </summary>
        public ProfileRightSideType ParameterType { get; private set; }

        /// <summary>
        ///     Gets the position at which the modifier appears on the data binding
        /// </summary>
        public int Order { get; internal set; }

        /// <summary>
        ///     Gets the currently used instance of the parameter data model
        /// </summary>
        public DataModel ParameterDataModel { get; private set; }

        /// <summary>
        ///     Gets the path of the parameter property in the <see cref="ParameterDataModel" />
        /// </summary>
        public string ParameterPropertyPath { get; private set; }

        /// <summary>
        ///     Gets the parameter static value, only used it <see cref="ParameterType" /> is
        ///     <see cref="ProfileRightSideType.Static" />
        /// </summary>
        public object ParameterStaticValue { get; private set; }

        /// <summary>
        ///     A compiled expression tree that when given a matching data model returns the value of the modifiers parameter
        /// </summary>
        public Func<DataModel, object> CompiledParameterAccessor { get; set; }

        internal DataBindingModifierEntity Entity { get; set; }


        /// <inheritdoc />
        public void Save()
        {
            if (_disposed)
                throw new ObjectDisposedException("DataBindingModifier");

            if (!DataBinding.Entity.Modifiers.Contains(Entity))
                DataBinding.Entity.Modifiers.Add(Entity);

            // Modifier
            if (ModifierType != null)
            {
                Entity.ModifierType = ModifierType.GetType().Name;
                Entity.ModifierTypePluginGuid = ModifierType.PluginInfo.Guid;
            }

            // General
            Entity.Order = Order;
            Entity.ParameterType = (int) ParameterType;

            // Parameter
            if (ParameterDataModel != null)
            {
                Entity.ParameterDataModelGuid = ParameterDataModel.PluginInfo.Guid;
                Entity.ParameterPropertyPath = ParameterPropertyPath;
            }

            Entity.ParameterStaticValue = JsonConvert.SerializeObject(ParameterStaticValue);
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

        /// <summary>
        ///     Applies the modifier to the provided value
        /// </summary>
        /// <param name="currentValue">The value to apply the modifier to, should be of the same type as the data binding target</param>
        /// <returns>The modified value</returns>
        public object Apply(object currentValue)
        {
            if (_disposed)
                throw new ObjectDisposedException("DataBindingModifier");

            if (ModifierType == null)
                return currentValue;

            if (!ModifierType.SupportsParameter)
                return ModifierType.Apply(currentValue, null);

            if (ParameterType == ProfileRightSideType.Dynamic && CompiledParameterAccessor != null)
            {
                var value = CompiledParameterAccessor(ParameterDataModel);
                return ModifierType.Apply(currentValue, value);
            }

            if (ParameterType == ProfileRightSideType.Static)
                return ModifierType.Apply(currentValue, ParameterStaticValue);

            return currentValue;
        }

        /// <summary>
        ///     Updates the modifier type of the modifier and re-compiles the expression
        /// </summary>
        /// <param name="modifierType"></param>
        public void UpdateModifierType(DataBindingModifierType modifierType)
        {
            if (_disposed)
                throw new ObjectDisposedException("DataBindingModifier");

            // Calling CreateExpression will clear compiled expressions
            if (modifierType == null)
            {
                ModifierType = null;
                CreateExpression();
                return;
            }

            var targetType = DataBinding.TargetProperty.PropertyType;
            if (!modifierType.SupportsType(targetType))
            {
                throw new ArtemisCoreException($"Cannot apply modifier type {modifierType.GetType().Name} to this modifier because " +
                                               $"it does not support this data binding's type {targetType.Name}");
            }

            ModifierType = modifierType;
            CreateExpression();
        }

        /// <summary>
        ///     Updates the parameter of the modifier, makes the modifier dynamic and re-compiles the expression
        /// </summary>
        /// <param name="dataModel">The data model of the parameter</param>
        /// <param name="path">The path pointing to the parameter inside the data model</param>
        public void UpdateParameter(DataModel dataModel, string path)
        {
            if (_disposed)
                throw new ObjectDisposedException("DataBindingModifier");

            if (dataModel != null && path == null)
                throw new ArtemisCoreException("If a data model is provided, a path is also required");
            if (dataModel == null && path != null)
                throw new ArtemisCoreException("If path is provided, a data model is also required");

            if (dataModel != null)
            {
                if (!dataModel.ContainsPath(path))
                    throw new ArtemisCoreException($"Data model of type {dataModel.GetType().Name} does not contain a property at path '{path}'");
            }

            ParameterType = ProfileRightSideType.Dynamic;
            ParameterDataModel = dataModel;
            ParameterPropertyPath = path;

            CreateExpression();
        }

        /// <summary>
        ///     Updates the parameter of the modifier, makes the modifier static and re-compiles the expression
        /// </summary>
        /// <param name="staticValue">The static value to use as a parameter</param>
        public void UpdateParameter(object staticValue)
        {
            if (_disposed)
                throw new ObjectDisposedException("DataBindingModifier");

            ParameterType = ProfileRightSideType.Static;
            ParameterDataModel = null;
            ParameterPropertyPath = null;

            var targetType = DataBinding.TargetProperty.PropertyType;

            // If not null ensure the types match and if not, convert it
            if (staticValue != null && staticValue.GetType() == targetType)
                ParameterStaticValue = staticValue;
            else if (staticValue != null)
                ParameterStaticValue = Convert.ChangeType(staticValue, targetType);
            // If null create a default instance for value types or simply make it null for reference types
            else if (targetType.IsValueType)
                ParameterStaticValue = Activator.CreateInstance(targetType);
            else
                ParameterStaticValue = null;

            CreateExpression();
        }

        private void Initialize()
        {
            DataBindingModifierTypeStore.DataBindingModifierAdded += DataBindingModifierTypeStoreOnDataBindingModifierAdded;
            DataBindingModifierTypeStore.DataBindingModifierRemoved += DataBindingModifierTypeStoreOnDataBindingModifierRemoved;
            DataModelStore.DataModelAdded += DataModelStoreOnDataModelAdded;
            DataModelStore.DataModelRemoved += DataModelStoreOnDataModelRemoved;

            // Modifier type
            if (Entity.ModifierTypePluginGuid != null && ModifierType == null)
            {
                var modifierType = DataBindingModifierTypeStore.Get(Entity.ModifierTypePluginGuid.Value, Entity.ModifierType)?.DataBindingModifierType;
                if (modifierType != null)
                    UpdateModifierType(modifierType);
            }

            // Dynamic parameter
            if (ParameterType == ProfileRightSideType.Dynamic && Entity.ParameterDataModelGuid != null && ParameterDataModel == null)
            {
                var dataModel = DataModelStore.Get(Entity.ParameterDataModelGuid.Value)?.DataModel;
                if (dataModel != null && dataModel.ContainsPath(Entity.ParameterPropertyPath))
                    UpdateParameter(dataModel, Entity.ParameterPropertyPath);
            }
            // Static parameter
            else if (ParameterType == ProfileRightSideType.Static && Entity.ParameterStaticValue != null && ParameterStaticValue == null)
            {
                // Use the target type so JSON.NET has a better idea what to do
                var targetType = DataBinding.TargetProperty.PropertyType;
                object staticValue;

                try
                {
                    staticValue = JsonConvert.DeserializeObject(Entity.ParameterStaticValue, targetType);
                }
                // If deserialization fails, use the type's default
                catch (JsonSerializationException e)
                {
                    DeserializationLogger.LogModifierDeserializationFailure(GetType().Name, e);
                    staticValue = Activator.CreateInstance(targetType);
                }

                UpdateParameter(staticValue);
            }
        }

        private void CreateExpression()
        {
            CompiledParameterAccessor = null;

            if (ModifierType == null || DataBinding == null)
                return;

            if (ParameterType == ProfileRightSideType.Dynamic && ModifierType.SupportsParameter)
            {
                if (ParameterDataModel == null)
                    return;

                // If the right side value is null, the constant type cannot be inferred and must be provided based on the data binding target
                var parameterAccessor = CreateAccessor(ParameterDataModel, ParameterPropertyPath, "parameter", out var rightSideParameter);
                var lambda = Expression.Lambda<Func<DataModel, object>>(Expression.Convert(parameterAccessor, typeof(object)), rightSideParameter);
                CompiledParameterAccessor = lambda.Compile();
            }
        }

        private Expression CreateAccessor(DataModel dataModel, string path, string parameterName, out ParameterExpression parameter)
        {
            var listType = dataModel.GetListTypeInPath(path);
            if (listType != null)
                throw new ArtemisCoreException($"Cannot create a regular accessor at path {path} because the path contains a list");

            parameter = Expression.Parameter(typeof(object), parameterName + "DataModel");
            return path.Split('.').Aggregate<string, Expression>(
                Expression.Convert(parameter, dataModel.GetType()), // Cast to the appropriate type
                Expression.Property
            );
        }

        #region Event handlers

        private void DataBindingModifierTypeStoreOnDataBindingModifierAdded(object sender, DataBindingModifierTypeStoreEvent e)
        {
            if (ModifierType != null)
                return;

            var modifierType = e.TypeRegistration.DataBindingModifierType;
            if (modifierType.PluginInfo.Guid == Entity.ModifierTypePluginGuid && modifierType.GetType().Name == Entity.ModifierType)
                UpdateModifierType(modifierType);
        }

        private void DataBindingModifierTypeStoreOnDataBindingModifierRemoved(object sender, DataBindingModifierTypeStoreEvent e)
        {
            if (e.TypeRegistration.DataBindingModifierType == ModifierType)
                UpdateModifierType(null);
        }

        private void DataModelStoreOnDataModelAdded(object sender, DataModelStoreEvent e)
        {
            var dataModel = e.Registration.DataModel;
            if (dataModel.PluginInfo.Guid == Entity.ParameterDataModelGuid && dataModel.ContainsPath(Entity.ParameterPropertyPath))
                UpdateParameter(dataModel, Entity.ParameterPropertyPath);
        }

        private void DataModelStoreOnDataModelRemoved(object sender, DataModelStoreEvent e)
        {
            if (e.Registration.DataModel != ParameterDataModel) 
                return;
            ParameterDataModel = null;
            CompiledParameterAccessor = null;
        }

        #endregion

        /// <inheritdoc />
        public void Dispose()
        {
            _disposed = true;

            DataBindingModifierTypeStore.DataBindingModifierAdded -= DataBindingModifierTypeStoreOnDataBindingModifierAdded;
            DataBindingModifierTypeStore.DataBindingModifierRemoved -= DataBindingModifierTypeStoreOnDataBindingModifierRemoved;
            DataModelStore.DataModelAdded -= DataModelStoreOnDataModelAdded;
            DataModelStore.DataModelRemoved -= DataModelStoreOnDataModelRemoved;
        }
    }
}
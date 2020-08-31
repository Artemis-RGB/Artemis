using System;
using System.Linq;
using System.Linq.Expressions;
using Artemis.Core.DataModelExpansions;
using Artemis.Core.Services;
using Artemis.Storage.Entities.Profile.DataBindings;
using Newtonsoft.Json;

namespace Artemis.Core
{
    /// <summary>
    ///     Modifies a data model value in a way defined by the modifier type
    /// </summary>
    public class DataBindingModifier
    {
        /// <summary>
        ///     Creates a new instance of the <see cref="DataBindingModifier" /> class
        /// </summary>
        public DataBindingModifier(DataBinding dataBinding, ProfileRightSideType parameterType)
        {
            DataBinding = dataBinding;
            ParameterType = parameterType;
            Entity = new DataBindingModifierEntity();
        }

        /// <summary>
        ///     Creates a new instance of the <see cref="DataBindingModifier" /> class
        /// </summary>
        public DataBindingModifier(DataBinding dataBinding, DataBindingModifierEntity entity)
        {
            DataBinding = dataBinding;
            ParameterType = (ProfileRightSideType) entity.ParameterType;
            Order = entity.Order;
            Entity = entity;
        }

        /// <summary>
        ///     Gets the data binding this modifier is applied to
        /// </summary>
        public DataBinding DataBinding { get; internal set; }

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
        ///     Gets the compiled function that evaluates this predicate if it of a dynamic <see cref="ParameterType" />
        /// </summary>
        public Func<object, DataModel, object> CompiledDynamicPredicate { get; private set; }

        /// <summary>
        ///     Gets the compiled function that evaluates this predicate if it is of a static <see cref="ParameterType" />
        /// </summary>
        public Func<object, object> CompiledStaticPredicate { get; private set; }

        internal DataBindingModifierEntity Entity { get; set; }

        /// <summary>
        ///     Applies the modifier to the provided value
        /// </summary>
        /// <param name="currentValue">The value to apply the modifier to, should be of the same type as the data binding target</param>
        /// <returns>The modified value</returns>
        public object Apply(object currentValue)
        {
            var targetType = DataBinding.Target.GetPropertyType();
            if (currentValue.GetType() != targetType)
            {
                throw new ArtemisCoreException("The current value of the data binding does not match the type of the target property." +
                                               $" {targetType.Name} expected, received {currentValue.GetType().Name}.");
            }

            if (CompiledDynamicPredicate != null)
                return CompiledDynamicPredicate(currentValue, ParameterDataModel);
            if (CompiledStaticPredicate != null)
                return CompiledStaticPredicate(currentValue);

            return currentValue;
        }

        /// <summary>
        ///     Updates the modifier type of the modifier and re-compiles the expression
        /// </summary>
        /// <param name="modifierType"></param>
        public void UpdateModifierType(DataBindingModifierType modifierType)
        {
            // Calling CreateExpression will clear compiled expressions
            if (modifierType == null)
            {
                ModifierType = null;
                CreateExpression();
                return;
            }

            var targetType = DataBinding.Target.GetPropertyType();
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
            ParameterType = ProfileRightSideType.Static;
            ParameterDataModel = null;
            ParameterPropertyPath = null;

            var targetType = DataBinding.Target.GetPropertyType();

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

        internal void Initialize(IDataModelService dataModelService, IDataBindingService dataBindingService)
        {
            // Modifier type
            if (Entity.ModifierTypePluginGuid != null)
            {
                var modifierType = dataBindingService.GetModifierType(Entity.ModifierTypePluginGuid.Value, Entity.ModifierType);
                if (modifierType != null)
                    UpdateModifierType(modifierType);
            }

            // Dynamic parameter
            if (ParameterType == ProfileRightSideType.Dynamic && Entity.ParameterDataModelGuid != null)
            {
                var dataModel = dataModelService.GetPluginDataModelByGuid(Entity.ParameterDataModelGuid.Value);
                if (dataModel != null && dataModel.ContainsPath(Entity.ParameterPropertyPath))
                    UpdateParameter(dataModel, Entity.ParameterPropertyPath);
            }
            else if (ParameterType == ProfileRightSideType.Static && Entity.ParameterStaticValue != null)
            {
                // Use the target type so JSON.NET has a better idea what to do
                var targetType = DataBinding.Target.GetPropertyType();
                object staticValue;

                try
                {
                    staticValue = JsonConvert.DeserializeObject(Entity.ParameterStaticValue, targetType);
                }
                // If deserialization fails, use the type's default
                catch (JsonSerializationException e)
                {
                    dataBindingService.LogModifierDeserializationFailure(this, e);
                    staticValue = Activator.CreateInstance(targetType);
                }

                UpdateParameter(staticValue);
            }

            // Static parameter
        }


        internal void CreateExpression()
        {
            CompiledDynamicPredicate = null;
            CompiledStaticPredicate = null;

            if (ModifierType == null || DataBinding == null)
                return;

            if (ParameterType == ProfileRightSideType.Dynamic && ModifierType.SupportsParameter)
                CreateDynamicExpression();
            else
                CreateStaticExpression();
        }

        private void CreateDynamicExpression()
        {
            if (ParameterDataModel == null)
                return;

            var currentValueParameter = Expression.Parameter(DataBinding.Target.GetPropertyType());

            // If the right side value is null, the constant type cannot be inferred and must be provided based on the data binding target
            var rightSideAccessor = CreateAccessor(ParameterDataModel, ParameterPropertyPath, "right", out var rightSideParameter);

            // A conversion may be required if the types differ
            // This can cause issues if the DisplayConditionOperator wasn't accurate in it's supported types but that is not a concern here
            if (rightSideAccessor.Type != DataBinding.Target.GetPropertyType())
                rightSideAccessor = Expression.Convert(rightSideAccessor, DataBinding.Target.GetPropertyType());

            var modifierExpression = ModifierType.CreateExpression(currentValueParameter, rightSideAccessor);
            var lambda = Expression.Lambda<Func<object, DataModel, object>>(modifierExpression, currentValueParameter, rightSideParameter);
            CompiledDynamicPredicate = lambda.Compile();
        }

        private void CreateStaticExpression()
        {
            var currentValueParameter = Expression.Parameter(DataBinding.Target.GetPropertyType());

            // If the right side value is null, the constant type cannot be inferred and must be provided based on the data binding target
            var rightSideConstant = ParameterStaticValue != null
                ? Expression.Constant(ParameterStaticValue)
                : Expression.Constant(null, DataBinding.Target.GetPropertyType());

            var modifierExpression = ModifierType.CreateExpression(currentValueParameter, rightSideConstant);
            var lambda = Expression.Lambda<Func<object, object>>(modifierExpression, currentValueParameter);
            CompiledStaticPredicate = lambda.Compile();
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
    }
}
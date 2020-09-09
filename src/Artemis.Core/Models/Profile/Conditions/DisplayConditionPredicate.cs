using System;
using System.Linq;
using System.Linq.Expressions;
using Artemis.Core.DataModelExpansions;
using Artemis.Storage.Entities.Profile.Abstract;
using Artemis.Storage.Entities.Profile.Conditions;
using Newtonsoft.Json;

namespace Artemis.Core
{
    /// <summary>
    ///     A predicate in a display condition using either two data model values or one data model value and a
    ///     static value
    /// </summary>
    public class DisplayConditionPredicate : DisplayConditionPart
    {
        /// <summary>
        ///     Creates a new instance of the <see cref="DisplayConditionPredicate" /> class
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="predicateType"></param>
        public DisplayConditionPredicate(DisplayConditionPart parent, ProfileRightSideType predicateType)
        {
            Parent = parent;
            PredicateType = predicateType;
            Entity = new DisplayConditionPredicateEntity();

            Initialize();
        }

        /// <summary>
        ///     Creates a new instance of the <see cref="DisplayConditionPredicate" /> class
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="entity"></param>
        public DisplayConditionPredicate(DisplayConditionPart parent, DisplayConditionPredicateEntity entity)
        {
            Parent = parent;
            Entity = entity;
            PredicateType = (ProfileRightSideType) entity.PredicateType;

            Initialize();
        }

        /// <summary>
        ///     Gets or sets the predicate type
        /// </summary>
        public ProfileRightSideType PredicateType { get; set; }

        /// <summary>
        ///     Gets the operator
        /// </summary>
        public ConditionOperator Operator { get; private set; }

        /// <summary>
        ///     Gets the currently used instance of the left data model
        /// </summary>
        public DataModel LeftDataModel { get; private set; }

        /// <summary>
        ///     Gets the path of the left property in the <see cref="LeftDataModel" />
        /// </summary>
        public string LeftPropertyPath { get; private set; }

        /// <summary>
        ///     Gets the currently used instance of the right data model
        /// </summary>
        public DataModel RightDataModel { get; private set; }

        /// <summary>
        ///     Gets the path of the right property in the <see cref="RightDataModel" />
        /// </summary>
        public string RightPropertyPath { get; private set; }

        /// <summary>
        ///     Gets the right static value, only used it <see cref="PredicateType" /> is
        ///     <see cref="ProfileRightSideType.Static" />
        /// </summary>
        public object RightStaticValue { get; private set; }

        /// <summary>
        ///     Gets the compiled function that evaluates this predicate if it of a dynamic <see cref="PredicateType" />
        /// </summary>
        public Func<DataModel, DataModel, bool> CompiledDynamicPredicate { get; private set; }

        /// <summary>
        ///     Gets the compiled function that evaluates this predicate if it is of a static <see cref="PredicateType" />
        /// </summary>
        public Func<DataModel, bool> CompiledStaticPredicate { get; private set; }

        internal DisplayConditionPredicateEntity Entity { get; set; }

        /// <summary>
        ///     Updates the left side of the predicate
        /// </summary>
        /// <param name="dataModel">The data model of the left side value</param>
        /// <param name="path">The path pointing to the left side value inside the data model</param>
        public void UpdateLeftSide(DataModel dataModel, string path)
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

            LeftDataModel = dataModel;
            LeftPropertyPath = path;

            ValidateOperator();
            ValidateRightSide();

            CreateExpression();
        }

        /// <summary>
        ///     Updates the right side of the predicate, makes the predicate dynamic and re-compiles the expression
        /// </summary>
        /// <param name="dataModel">The data model of the right side value</param>
        /// <param name="path">The path pointing to the right side value inside the data model</param>
        public void UpdateRightSide(DataModel dataModel, string path)
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

            PredicateType = ProfileRightSideType.Dynamic;
            RightDataModel = dataModel;
            RightPropertyPath = path;

            CreateExpression();
        }

        /// <summary>
        ///     Updates the right side of the predicate, makes the predicate static and re-compiles the expression
        /// </summary>
        /// <param name="staticValue">The right side value to use</param>
        public void UpdateRightSide(object staticValue)
        {
            PredicateType = ProfileRightSideType.Static;
            RightDataModel = null;
            RightPropertyPath = null;

            // If the left side is empty simply apply the value, any validation will wait
            if (LeftDataModel == null)
            {
                RightStaticValue = staticValue;
                return;
            }

            var leftSideType = LeftDataModel.GetTypeAtPath(LeftPropertyPath);

            // If not null ensure the types match and if not, convert it
            if (staticValue != null && staticValue.GetType() == leftSideType)
                RightStaticValue = staticValue;
            else if (staticValue != null)
                RightStaticValue = Convert.ChangeType(staticValue, leftSideType);
            // If null create a default instance for value types or simply make it null for reference types
            else if (leftSideType.IsValueType)
                RightStaticValue = Activator.CreateInstance(leftSideType);
            else
                RightStaticValue = null;

            CreateExpression();
        }

        /// <summary>
        ///     Updates the operator of the predicate and re-compiles the expression
        /// </summary>
        /// <param name="conditionOperator"></param>
        public void UpdateOperator(ConditionOperator conditionOperator)
        {
            // Calling CreateExpression will clear compiled expressions
            if (conditionOperator == null)
            {
                Operator = null;
                CreateExpression();
                return;
            }

            // No need to clear compiled expressions, without a left data model they are already null
            if (LeftDataModel == null)
            {
                Operator = conditionOperator;
                return;
            }

            var leftType = LeftDataModel.GetTypeAtPath(LeftPropertyPath);
            if (!conditionOperator.SupportsType(leftType))
            {
                throw new ArtemisCoreException($"Cannot apply operator {conditionOperator.GetType().Name} to this predicate because " +
                                               $"it does not support left side type {leftType.Name}");
            }

            Operator = conditionOperator;
            CreateExpression();
        }

        /// <inheritdoc />
        public override bool Evaluate()
        {
            if (CompiledDynamicPredicate != null)
                return CompiledDynamicPredicate(LeftDataModel, RightDataModel);
            if (CompiledStaticPredicate != null)
                return CompiledStaticPredicate(LeftDataModel);

            return false;
        }

        /// <inheritdoc />
        public override bool EvaluateObject(object target)
        {
            return false;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            if (PredicateType == ProfileRightSideType.Dynamic)
                return $"[Dynamic] {LeftPropertyPath} {Operator.Description} {RightPropertyPath}";
            return $"[Static] {LeftPropertyPath} {Operator.Description} {RightStaticValue}";
        }

        internal override void ApplyToEntity()
        {
            Entity.PredicateType = (int) PredicateType;
            Entity.LeftDataModelGuid = LeftDataModel?.PluginInfo?.Guid;
            Entity.LeftPropertyPath = LeftPropertyPath;

            Entity.RightDataModelGuid = RightDataModel?.PluginInfo?.Guid;
            Entity.RightPropertyPath = RightPropertyPath;
            Entity.RightStaticValue = JsonConvert.SerializeObject(RightStaticValue);

            Entity.OperatorPluginGuid = Operator?.PluginInfo?.Guid;
            Entity.OperatorType = Operator?.GetType().Name;
        }

        internal void Initialize()
        {
            DataModelStore.DataModelAdded += DataModelStoreOnDataModelAdded;
            DataModelStore.DataModelRemoved += DataModelStoreOnDataModelRemoved;

            // Left side
            if (Entity.LeftDataModelGuid != null)
            {
                var dataModel = DataModelStore.Get(Entity.LeftDataModelGuid.Value)?.DataModel;
                if (dataModel != null && dataModel.ContainsPath(Entity.LeftPropertyPath))
                    UpdateLeftSide(dataModel, Entity.LeftPropertyPath);
            }

            // Operator
            if (Entity.OperatorPluginGuid != null)
            {
                var conditionOperator = ConditionOperatorStore.Get(Entity.OperatorPluginGuid.Value, Entity.OperatorType)?.ConditionOperator;
                if (conditionOperator != null)
                    UpdateOperator(conditionOperator);
            }

            // Right side dynamic
            if (PredicateType == ProfileRightSideType.Dynamic && Entity.RightDataModelGuid != null)
            {
                var dataModel = DataModelStore.Get(Entity.RightDataModelGuid.Value)?.DataModel;
                if (dataModel != null && dataModel.ContainsPath(Entity.RightPropertyPath))
                    UpdateRightSide(dataModel, Entity.RightPropertyPath);
            }
            // Right side static
            else if (PredicateType == ProfileRightSideType.Static && Entity.RightStaticValue != null)
            {
                try
                {
                    if (LeftDataModel != null)
                    {
                        // Use the left side type so JSON.NET has a better idea what to do
                        var leftSideType = LeftDataModel.GetTypeAtPath(LeftPropertyPath);
                        object rightSideValue;

                        try
                        {
                            rightSideValue = JsonConvert.DeserializeObject(Entity.RightStaticValue, leftSideType);
                        }
                        // If deserialization fails, use the type's default
                        catch (JsonSerializationException e)
                        {
                            DeserializationLogger.LogPredicateDeserializationFailure(this, e);
                            rightSideValue = Activator.CreateInstance(leftSideType);
                        }

                        UpdateRightSide(rightSideValue);
                    }
                    else
                    {
                        // Hope for the best...
                        UpdateRightSide(JsonConvert.DeserializeObject(Entity.RightStaticValue));
                    }
                }
                catch (JsonReaderException)
                {
                    // ignored
                    // TODO: Some logging would be nice
                }
            }
        }

        internal override DisplayConditionPartEntity GetEntity()
        {
            return Entity;
        }

        private void CreateExpression()
        {
            CompiledDynamicPredicate = null;
            CompiledStaticPredicate = null;

            if (Operator == null)
                return;

            // If the operator does not support a right side, create a static expression because the right side will simply be null
            if (PredicateType == ProfileRightSideType.Dynamic && Operator.SupportsRightSide)
                CreateDynamicExpression();
            else
                CreateStaticExpression();
        }

        private void ValidateOperator()
        {
            if (LeftDataModel == null || Operator == null)
                return;

            var leftType = LeftDataModel.GetTypeAtPath(LeftPropertyPath);
            if (!Operator.SupportsType(leftType))
                Operator = null;
        }

        private void ValidateRightSide()
        {
            var leftSideType = LeftDataModel.GetTypeAtPath(LeftPropertyPath);
            if (PredicateType == ProfileRightSideType.Dynamic)
            {
                if (RightDataModel == null)
                    return;

                var rightSideType = RightDataModel.GetTypeAtPath(RightPropertyPath);
                if (!leftSideType.IsCastableFrom(rightSideType))
                    UpdateRightSide(null, null);
            }
            else
            {
                if (RightStaticValue != null && leftSideType.IsCastableFrom(RightStaticValue.GetType()))
                    UpdateRightSide(RightStaticValue);
                else
                    UpdateRightSide(null);
            }
        }

        private void CreateDynamicExpression()
        {
            if (LeftDataModel == null || RightDataModel == null || Operator == null)
                return;

            var leftSideAccessor = CreateAccessor(LeftDataModel, LeftPropertyPath, "left", out var leftSideParameter);
            var rightSideAccessor = CreateAccessor(RightDataModel, RightPropertyPath, "right", out var rightSideParameter);

            // A conversion may be required if the types differ
            // This can cause issues if the DisplayConditionOperator wasn't accurate in it's supported types but that is not a concern here
            if (rightSideAccessor.Type != leftSideAccessor.Type)
                rightSideAccessor = Expression.Convert(rightSideAccessor, leftSideAccessor.Type);

            var conditionExpression = Operator.CreateExpression(leftSideAccessor, rightSideAccessor);
            var lambda = Expression.Lambda<Func<DataModel, DataModel, bool>>(conditionExpression, leftSideParameter, rightSideParameter);
            CompiledDynamicPredicate = lambda.Compile();
        }

        private void CreateStaticExpression()
        {
            if (LeftDataModel == null || Operator == null)
                return;

            var leftSideAccessor = CreateAccessor(LeftDataModel, LeftPropertyPath, "left", out var leftSideParameter);

            // If the left side is a value type but the input is empty, this isn't a valid expression
            if (leftSideAccessor.Type.IsValueType && RightStaticValue == null)
                return;

            // If the right side value is null, the constant type cannot be inferred and must be provided manually
            var rightSideConstant = RightStaticValue != null
                ? Expression.Constant(RightStaticValue)
                : Expression.Constant(null, leftSideAccessor.Type);

            var conditionExpression = Operator.CreateExpression(leftSideAccessor, rightSideConstant);
            var lambda = Expression.Lambda<Func<DataModel, bool>>(conditionExpression, leftSideParameter);
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

        private Expression CreateListAccessor(DataModel dataModel, string path, ParameterExpression listParameter)
        {
            var listType = dataModel.GetListTypeInPath(path);
            if (listType == null)
                throw new ArtemisCoreException($"Cannot create a list accessor at path {path} because the path does not contain a list");

            path = dataModel.GetListInnerPath(path);
            return path.Split('.').Aggregate<string, Expression>(
                Expression.Convert(listParameter, listType), // Cast to the appropriate type
                Expression.Property
            );
        }

        #region Event handlers

        private void DataModelStoreOnDataModelAdded(object sender, DataModelStoreEvent e)
        {
            var dataModel = e.Registration.DataModel;
            if (dataModel.PluginInfo.Guid == Entity.LeftDataModelGuid && dataModel.ContainsPath(Entity.LeftPropertyPath))
                UpdateLeftSide(dataModel, Entity.LeftPropertyPath);
            if (dataModel.PluginInfo.Guid == Entity.RightDataModelGuid && dataModel.ContainsPath(Entity.RightPropertyPath))
                UpdateRightSide(dataModel, Entity.RightPropertyPath);
        }

        private void DataModelStoreOnDataModelRemoved(object sender, DataModelStoreEvent e)
        {
            if (LeftDataModel == e.Registration.DataModel)
            {
                CompiledDynamicPredicate = null;
                LeftDataModel = null;
            }

            if (RightDataModel == e.Registration.DataModel)
            {
                CompiledDynamicPredicate = null;
                RightDataModel = null;
            }
        }

        #endregion

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            DataModelStore.DataModelAdded -= DataModelStoreOnDataModelAdded;
            DataModelStore.DataModelRemoved -= DataModelStoreOnDataModelRemoved;

            base.Dispose(disposing);
        }
    }
}
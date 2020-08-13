using System;
using System.Linq;
using System.Linq.Expressions;
using Artemis.Core.Exceptions;
using Artemis.Core.Extensions;
using Artemis.Core.Models.Profile.Conditions.Abstract;
using Artemis.Core.Plugins.Abstract.DataModels;
using Artemis.Core.Services.Interfaces;
using Artemis.Storage.Entities.Profile;
using Artemis.Storage.Entities.Profile.Abstract;
using Newtonsoft.Json;

namespace Artemis.Core.Models.Profile.Conditions
{
    public class DisplayConditionPredicate : DisplayConditionPart
    {
        public DisplayConditionPredicate(DisplayConditionPart parent, PredicateType predicateType)
        {
            Parent = parent;
            PredicateType = predicateType;
            Entity = new DisplayConditionPredicateEntity();
        }

        public DisplayConditionPredicate(DisplayConditionPart parent, DisplayConditionPredicateEntity entity)
        {
            Parent = parent;
            Entity = entity;
            PredicateType = (PredicateType) entity.PredicateType;
        }

        public DisplayConditionPredicateEntity Entity { get; set; }

        public PredicateType PredicateType { get; set; }
        public DisplayConditionOperator Operator { get; private set; }

        public DataModel LeftDataModel { get; private set; }
        public string LeftPropertyPath { get; private set; }
        public DataModel RightDataModel { get; private set; }
        public string RightPropertyPath { get; private set; }
        public object RightStaticValue { get; private set; }
        public DataModel ListDataModel { get; private set; }
        public string ListPropertyPath { get; private set; }

        public Func<DataModel, DataModel, bool> CompiledDynamicPredicate { get; private set; }
        public Func<DataModel, bool> CompiledStaticPredicate { get; private set; }
        public Func<object, bool> CompiledListPredicate { get; private set; }

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

            PredicateType = PredicateType.Dynamic;
            RightDataModel = dataModel;
            RightPropertyPath = path;

            CreateExpression();
        }

        public void UpdateRightSide(object staticValue)
        {
            PredicateType = PredicateType.Static;
            RightDataModel = null;
            RightPropertyPath = null;

            SetStaticValue(staticValue);

            CreateExpression();
        }

        public void UpdateOperator(DisplayConditionOperator displayConditionOperator)
        {
            if (displayConditionOperator == null)
            {
                Operator = null;
                return;
            }

            if (LeftDataModel == null)
            {
                Operator = displayConditionOperator;
                return;
            }

            var leftType = LeftDataModel.GetTypeAtPath(LeftPropertyPath);
            if (displayConditionOperator.SupportsType(leftType))
                Operator = displayConditionOperator;

            CreateExpression();
        }

        private void CreateExpression()
        {
            CompiledDynamicPredicate = null;
            CompiledStaticPredicate = null;
            CompiledListPredicate = null;

            if (Operator == null)
                return;

            // If the operator does not support a right side, create a static expression because the right side will simply be null
            if (PredicateType == PredicateType.Dynamic && Operator.SupportsRightSide)
                CreateDynamicExpression();

            CreateStaticExpression();
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

        public override bool Evaluate()
        {
            if (CompiledDynamicPredicate != null)
                return CompiledDynamicPredicate(LeftDataModel, RightDataModel);
            if (CompiledStaticPredicate != null)
                return CompiledStaticPredicate(LeftDataModel);

            return false;
        }

        public override bool EvaluateObject(object target)
        {
            if (CompiledListPredicate != null)
                return CompiledListPredicate(target);

            return false;
        }

        internal override void Initialize(IDataModelService dataModelService)
        {
            // Left side
            if (Entity.LeftDataModelGuid != null)
            {
                var dataModel = dataModelService.GetPluginDataModelByGuid(Entity.LeftDataModelGuid.Value);
                if (dataModel != null && dataModel.ContainsPath(Entity.LeftPropertyPath))
                    UpdateLeftSide(dataModel, Entity.LeftPropertyPath);
            }

            // Operator
            if (Entity.OperatorPluginGuid != null)
            {
                var conditionOperator = dataModelService.GetConditionOperator(Entity.OperatorPluginGuid.Value, Entity.OperatorType);
                if (conditionOperator != null)
                    UpdateOperator(conditionOperator);
            }

            // Right side dynamic
            if (PredicateType == PredicateType.Dynamic && Entity.RightDataModelGuid != null)
            {
                var dataModel = dataModelService.GetPluginDataModelByGuid(Entity.RightDataModelGuid.Value);
                if (dataModel != null && dataModel.ContainsPath(Entity.RightPropertyPath))
                    UpdateRightSide(dataModel, Entity.RightPropertyPath);
            }
            // Right side static
            else if (PredicateType == PredicateType.Static && Entity.RightStaticValue != null)
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
                            dataModelService.LogPredicateDeserializationFailure(this, e);
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
            if (PredicateType == PredicateType.Dynamic)
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

        private void SetStaticValue(object staticValue)
        {
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
        }

        private void CreateDynamicExpression()
        {
            if (LeftDataModel == null || RightDataModel == null || Operator == null)
                return;

            var isListExpression = LeftDataModel.GetListTypeInPath(LeftPropertyPath) != null;

            Expression leftSideAccessor;
            Expression rightSideAccessor;
            ParameterExpression leftSideParameter;
            ParameterExpression rightSideParameter = null;
            if (isListExpression)
            {
                // List accessors share the same parameter because a list always contains one item per entry
                leftSideParameter = Expression.Parameter(typeof(object), "listItem");
                leftSideAccessor = CreateListAccessor(LeftDataModel, LeftPropertyPath, leftSideParameter);
                rightSideAccessor = CreateListAccessor(RightDataModel, RightPropertyPath, leftSideParameter);
            }
            else
            {
                leftSideAccessor = CreateAccessor(LeftDataModel, LeftPropertyPath, "left", out leftSideParameter);
                rightSideAccessor = CreateAccessor(RightDataModel, RightPropertyPath, "right", out rightSideParameter);
            }
            
            // A conversion may be required if the types differ
            // This can cause issues if the DisplayConditionOperator wasn't accurate in it's supported types but that is not a concern here
            if (rightSideAccessor.Type != leftSideAccessor.Type)
                rightSideAccessor = Expression.Convert(rightSideAccessor, leftSideAccessor.Type);

            var conditionExpression = Operator.CreateExpression(leftSideAccessor, rightSideAccessor);

            if (isListExpression)
            {
                var lambda = Expression.Lambda<Func<object, bool>>(conditionExpression, leftSideParameter);
                CompiledListPredicate = lambda.Compile();
            }
            else
            {
                var lambda = Expression.Lambda<Func<DataModel, DataModel, bool>>(conditionExpression, leftSideParameter, rightSideParameter);
                CompiledDynamicPredicate = lambda.Compile();
            }
        }

        private void CreateStaticExpression()
        {
            if (LeftDataModel == null || Operator == null)
                return;

            var isListExpression = LeftDataModel.GetListTypeInPath(LeftPropertyPath) != null;

            Expression leftSideAccessor;
            ParameterExpression leftSideParameter;
            if (isListExpression)
            {
                // List accessors share the same parameter because a list always contains one item per entry
                leftSideParameter = Expression.Parameter(typeof(object), "listItem");
                leftSideAccessor = CreateListAccessor(LeftDataModel, LeftPropertyPath, leftSideParameter);
            }
            else
                leftSideAccessor = CreateAccessor(LeftDataModel, LeftPropertyPath, "left", out leftSideParameter);

            // If the left side is a value type but the input is empty, this isn't a valid expression
            if (leftSideAccessor.Type.IsValueType && RightStaticValue == null)
                return;

            // If the right side value is null, the constant type cannot be inferred and must be provided manually
            var rightSideConstant = RightStaticValue != null
                ? Expression.Constant(RightStaticValue)
                : Expression.Constant(null, leftSideAccessor.Type);

            var conditionExpression = Operator.CreateExpression(leftSideAccessor, rightSideConstant);

            if (isListExpression)
            {
                var lambda = Expression.Lambda<Func<object, bool>>(conditionExpression, leftSideParameter);
                CompiledListPredicate = lambda.Compile();
            }
            else
            {
                var lambda = Expression.Lambda<Func<DataModel, bool>>(conditionExpression, leftSideParameter);
                CompiledStaticPredicate = lambda.Compile();
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

        public override string ToString()
        {
            if (PredicateType == PredicateType.Dynamic)
                return $"[Dynamic] {LeftPropertyPath} {Operator.Description} {RightPropertyPath}";
            return $"[Static] {LeftPropertyPath} {Operator.Description} {RightStaticValue}";
        }
    }

    public enum PredicateType
    {
        Static,
        Dynamic
    }
}
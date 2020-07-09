using System;
using System.Linq;
using System.Linq.Expressions;
using Artemis.Core.Exceptions;
using Artemis.Core.Extensions;
using Artemis.Core.Models.Profile.Conditions.Abstract;
using Artemis.Core.Plugins.Abstract.DataModels;
using Artemis.Storage.Entities.Profile;

namespace Artemis.Core.Models.Profile.Conditions
{
    public class DisplayConditionPredicate : DisplayConditionPart
    {
        public DisplayConditionPredicate(DisplayConditionPart parent)
        {
            Parent = parent;
            DisplayConditionPredicateEntity = new DisplayConditionPredicateEntity();
        }

        public DisplayConditionPredicate(DisplayConditionPart parent, DisplayConditionPredicateEntity entity)
        {
            Parent = parent;
            DisplayConditionPredicateEntity = entity;

            // TODO: This has to be done from somewhere
            // LeftDataModel = dataModelService.GetPluginDataModelByGuid(DisplayConditionPredicateEntity.LeftDataModelGuid);
            // RightDataModel = dataModelService.GetPluginDataModelByGuid(DisplayConditionPredicateEntity.RightDataModelGuid);
        }

        public DisplayConditionPredicateEntity DisplayConditionPredicateEntity { get; set; }

        public PredicateType PredicateType { get; set; }
        public DisplayConditionOperator Operator { get; set; }

        public DataModel LeftDataModel { get; private set; }
        public string LeftPropertyPath { get; private set; }
        public DataModel RightDataModel { get; private set; }
        public string RightPropertyPath { get; private set; }
        public object RightStaticValue { get; private set; }

        public Expression<Func<DataModel, DataModel, bool>> DynamicConditionLambda { get; private set; }
        public Func<DataModel, DataModel, bool> CompiledDynamicConditionLambda { get; private set; }
        public Expression<Func<DataModel, bool>> StaticConditionLambda { get; private set; }
        public Func<DataModel, bool> CompiledStaticConditionLambda { get; private set; }

        public void UpdateLeftSide(DataModel dataModel, string path)
        {
            if (!dataModel.ContainsPath(path))
                throw new ArtemisCoreException($"Data model of type {dataModel.GetType().Name} does not contain a property at path '{path}'");

            LeftDataModel = dataModel;
            LeftPropertyPath = path;

            ValidateRightSide();
            CreateExpression();
        }

        public void UpdateRightSide(DataModel dataModel, string path)
        {
            if (!dataModel.ContainsPath(path))
                throw new ArtemisCoreException($"Data model of type {dataModel.GetType().Name} does not contain a property at path '{path}'");

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

        public void CreateExpression()
        {
            if (PredicateType == PredicateType.Dynamic)
                CreateDynamicExpression();
            else
                CreateStaticExpression();
        }

        public override void ApplyToEntity()
        {
        }

        /// <summary>
        ///     Validates the right side, ensuring it is still compatible with the current left side
        /// </summary>
        private void ValidateRightSide()
        {
            var leftSideType = LeftDataModel.GetTypeAtPath(LeftPropertyPath);
            if (PredicateType == PredicateType.Dynamic)
            {
                var rightSideType = RightDataModel.GetTypeAtPath(RightPropertyPath);
                if (!leftSideType.IsCastableFrom(rightSideType))
                    UpdateRightSide(null, null);
            }
            else
            {
                // Just update the value with itself, it'll validate :)
                UpdateRightSide(RightStaticValue);
            }
        }

        /// <summary>
        ///     Updates the current static value, ensuring it is a valid type. This assumes the types are compatible if they
        ///     differ.
        /// </summary>
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
            if (LeftDataModel == null || RightDataModel == null)
                return;

            var leftSideParameter = Expression.Parameter(typeof(DataModel), "leftDataModel");
            var leftSideAccessor = LeftPropertyPath.Split('.').Aggregate<string, Expression>(
                Expression.Convert(leftSideParameter, LeftDataModel.GetType()), // Cast to the appropriate type
                Expression.Property
            );
            var rightSideParameter = Expression.Parameter(typeof(DataModel), "rightDataModel");
            var rightSideAccessor = RightPropertyPath.Split('.').Aggregate<string, Expression>(
                Expression.Convert(rightSideParameter, LeftDataModel.GetType()), // Cast to the appropriate type
                Expression.Property
            );

            // A conversion may be required if the types differ
            // This can cause issues if the DisplayConditionOperator wasn't accurate in it's supported types but that is not a concern here
            if (rightSideAccessor.Type != leftSideAccessor.Type)
                rightSideAccessor = Expression.Convert(rightSideAccessor, leftSideAccessor.Type);

            var dynamicConditionExpression = Operator.CreateExpression(leftSideAccessor, rightSideAccessor);

            DynamicConditionLambda = Expression.Lambda<Func<DataModel, DataModel, bool>>(dynamicConditionExpression, leftSideParameter, rightSideParameter);
            CompiledDynamicConditionLambda = DynamicConditionLambda.Compile();
        }

        private void CreateStaticExpression()
        {
            if (LeftDataModel == null)
                return;

            var leftSideParameter = Expression.Parameter(typeof(DataModel), "leftDataModel");
            var leftSideAccessor = LeftPropertyPath.Split('.').Aggregate<string, Expression>(
                Expression.Convert(leftSideParameter, LeftDataModel.GetType()), // Cast to the appropriate type
                Expression.Property
            );

            // If the left side is a value type but the input is empty, this isn't a valid expression
            if (leftSideAccessor.Type.IsValueType && RightStaticValue == null)
                return;

            var rightSideConstant = Expression.Constant(RightStaticValue);
            var conditionExpression = Operator.CreateExpression(leftSideAccessor, rightSideConstant);

            StaticConditionLambda = Expression.Lambda<Func<DataModel, bool>>(conditionExpression, leftSideParameter);
            CompiledStaticConditionLambda = StaticConditionLambda.Compile();
        }
    }

    public enum PredicateType
    {
        Static,
        Dynamic
    }
}
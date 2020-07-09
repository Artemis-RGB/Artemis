using System;
using System.Linq;
using System.Linq.Expressions;
using Artemis.Core.Models.Profile.Conditions.Abstract;
using Artemis.Core.Plugins.Abstract.DataModels;
using Artemis.Core.Services.Interfaces;
using Artemis.Storage.Entities.Profile;

namespace Artemis.Core.Models.Profile.Conditions
{
    public class DisplayConditionPredicate : DisplayConditionPart
    {
        public DisplayConditionPredicate(DisplayConditionPart parent)
        {
            Parent = parent;
        }

        public DisplayConditionPredicate(DisplayConditionPart parent, DisplayConditionPredicateEntity entity)
        {
            Parent = parent;
            DisplayConditionPredicateEntity = entity;
        }

        public DisplayConditionPredicateEntity DisplayConditionPredicateEntity { get; set; }

        public PredicateType PredicateType { get; set; }
        public DisplayConditionOperator Operator { get; set; }

        public Guid LeftDataModelGuid { get; set; }
        public string LeftPropertyPath { get; set; }

        public Guid RightDataModelGuid { get; set; }
        public string RightPropertyPath { get; set; }

        // TODO: Implement type-checking or perhaps convert it here
        public object RightStaticValue { get; set; }

        public Expression<Func<DataModel, DataModel, bool>> DynamicConditionLambda { get; private set; }
        public Func<DataModel, DataModel, bool> CompiledDynamicConditionLambda { get; private set; }
        public Expression<Func<DataModel, bool>> StaticConditionLambda { get; private set; }
        public Func<DataModel, bool> CompiledStaticConditionLambda { get; private set; }

        public void CreateExpression(IDataModelService dataModelService)
        {
            if (PredicateType == PredicateType.Dynamic)
                CreateDynamicExpression(dataModelService);
            else
                CreateStaticExpression(dataModelService);
        }

        private void CreateDynamicExpression(IDataModelService dataModelService)
        {
            if (LeftDataModelGuid == Guid.Empty || string.IsNullOrWhiteSpace(LeftPropertyPath))
                return;
            if (RightDataModelGuid == Guid.Empty || string.IsNullOrWhiteSpace(RightPropertyPath))
                return;

            var leftDataModel = dataModelService.GetPluginDataModelByGuid(LeftDataModelGuid);
            if (leftDataModel == null)
                return;

            var rightDataModel = dataModelService.GetPluginDataModelByGuid(RightDataModelGuid);
            if (rightDataModel == null)
                return;

            var leftSideParameter = Expression.Parameter(typeof(DataModel), "leftDataModel");
            var leftSideAccessor = LeftPropertyPath.Split('.').Aggregate<string, Expression>(
                Expression.Convert(leftSideParameter, leftDataModel.GetType()), // Cast to the appropriate type
                Expression.Property
            );
            var rightSideParameter = Expression.Parameter(typeof(DataModel), "rightDataModel");
            var rightSideAccessor = RightPropertyPath.Split('.').Aggregate<string, Expression>(
                Expression.Convert(rightSideParameter, rightDataModel.GetType()), // Cast to the appropriate type
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

        private void CreateStaticExpression(IDataModelService dataModelService)
        {
            if (LeftDataModelGuid == Guid.Empty || string.IsNullOrWhiteSpace(LeftPropertyPath))
                return;

            var leftDataModel = dataModelService.GetPluginDataModelByGuid(LeftDataModelGuid);
            if (leftDataModel == null)
                return;

            var leftSideParameter = Expression.Parameter(typeof(DataModel), "leftDataModel");
            var leftSideAccessor = LeftPropertyPath.Split('.').Aggregate<string, Expression>(
                Expression.Convert(leftSideParameter, leftDataModel.GetType()), // Cast to the appropriate type
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

        public override void ApplyToEntity()
        {
            
        }
    }

    public enum PredicateType
    {
        Static,
        Dynamic
    }
}
using System;
using System.Linq;
using System.Linq.Expressions;
using Artemis.Core.DataModelExpansions;
using Artemis.Storage.Entities.Profile.Abstract;
using Artemis.Storage.Entities.Profile.Conditions;
using Newtonsoft.Json;

namespace Artemis.Core
{
    public class DisplayConditionListPredicate : DisplayConditionPart
    {
        public DisplayConditionListPredicate(DisplayConditionPart parent, ProfileRightSideType predicateType)
        {
            Parent = parent;
            PredicateType = predicateType;
            Entity = new DisplayConditionListPredicateEntity();

            ApplyParentList();
            Initialize();
        }

        public DisplayConditionListPredicate(DisplayConditionPart parent, DisplayConditionListPredicateEntity entity)
        {
            Parent = parent;
            Entity = entity;
            PredicateType = (ProfileRightSideType) entity.PredicateType;

            ApplyParentList();
            Initialize();
        }

        public DisplayConditionListPredicateEntity Entity { get; set; }

        public ProfileRightSideType PredicateType { get; set; }
        public ConditionOperator Operator { get; private set; }

        public Type ListType { get; private set; }
        public DataModel ListDataModel { get; private set; }
        public string ListPropertyPath { get; private set; }

        public string LeftPropertyPath { get; private set; }
        public string RightPropertyPath { get; private set; }
        public object RightStaticValue { get; private set; }

        public Func<object, bool> CompiledListPredicate { get; private set; }

        public void ApplyParentList()
        {
            var current = Parent;
            while (current != null)
            {
                if (current is DisplayConditionList parentList)
                {
                    UpdateList(parentList.ListDataModel, parentList.ListPropertyPath);
                    return;
                }

                current = current.Parent;
            }
        }

        public void UpdateList(DataModel dataModel, string path)
        {
            if (dataModel != null && path == null)
                throw new ArtemisCoreException("If a data model is provided, a path is also required");
            if (dataModel == null && path != null)
                throw new ArtemisCoreException("If path is provided, a data model is also required");

            if (dataModel != null)
            {
                var listType = dataModel.GetListTypeAtPath(path);
                if (listType == null)
                    throw new ArtemisCoreException($"Data model of type {dataModel.GetType().Name} does not contain a list at path '{path}'");

                ListType = listType;
            }
            else
                ListType = null;

            ListDataModel = dataModel;
            ListPropertyPath = path;

            if (LeftPropertyPath != null && !ListContainsInnerPath(LeftPropertyPath))
                LeftPropertyPath = null;
            if (RightPropertyPath != null && !ListContainsInnerPath(RightPropertyPath))
                RightPropertyPath = null;

            CreateExpression();
        }

        public void UpdateLeftSide(string path)
        {
            if (!ListContainsInnerPath(path))
                throw new ArtemisCoreException($"List type {ListType.Name} does not contain path {path}");

            LeftPropertyPath = path;

            ValidateOperator();
            ValidateRightSide();
            CreateExpression();
        }

        public void UpdateRightSideDynamic(string path)
        {
            if (!ListContainsInnerPath(path))
                throw new ArtemisCoreException($"List type {ListType.Name} does not contain path {path}");

            PredicateType = ProfileRightSideType.Dynamic;
            RightPropertyPath = path;

            CreateExpression();
        }

        public void UpdateRightSideStatic(object staticValue)
        {
            PredicateType = ProfileRightSideType.Static;
            RightPropertyPath = null;

            SetStaticValue(staticValue);
            CreateExpression();
        }

        public void UpdateOperator(ConditionOperator conditionOperator)
        {
            if (conditionOperator == null)
            {
                Operator = null;
                return;
            }

            if (LeftPropertyPath == null)
            {
                Operator = conditionOperator;
                return;
            }

            var leftType = GetTypeAtInnerPath(LeftPropertyPath);
            if (conditionOperator.SupportsType(leftType))
                Operator = conditionOperator;

            CreateExpression();
        }

        public override bool Evaluate()
        {
            return false;
        }

        public override bool EvaluateObject(object target)
        {
            return CompiledListPredicate != null && CompiledListPredicate(target);
        }

        public bool ListContainsInnerPath(string path)
        {
            if (ListType == null)
                return false;

            var parts = path.Split('.');
            var current = ListType;
            foreach (var part in parts)
            {
                var property = current.GetProperty(part);
                current = property?.PropertyType;

                if (property == null)
                    return false;
            }

            return true;
        }

        public Type GetTypeAtInnerPath(string path)
        {
            if (!ListContainsInnerPath(path))
                return null;

            var parts = path.Split('.');
            var current = ListType;

            Type result = null;
            foreach (var part in parts)
            {
                var property = current.GetProperty(part);
                current = property.PropertyType;
                result = property.PropertyType;
            }

            return result;
        }

        internal override void Save()
        {
            Entity.PredicateType = (int) PredicateType;
            if (ListDataModel != null)
            {
                Entity.ListDataModelGuid = ListDataModel.PluginInfo.Guid;
                Entity.ListPropertyPath = ListPropertyPath;
            }

            Entity.LeftPropertyPath = LeftPropertyPath;
            Entity.RightPropertyPath = RightPropertyPath;
            Entity.RightStaticValue = JsonConvert.SerializeObject(RightStaticValue);

            if (Operator != null)
            {
                Entity.OperatorPluginGuid = Operator.PluginInfo.Guid;
                Entity.OperatorType = Operator.GetType().Name;
            }
        }

        private void Initialize()
        {
            ConditionOperatorStore.ConditionOperatorAdded += ConditionOperatorStoreOnConditionOperatorAdded;
            ConditionOperatorStore.ConditionOperatorRemoved += ConditionOperatorStoreOnConditionOperatorRemoved;

            // Left side
            if (Entity.LeftPropertyPath != null && ListContainsInnerPath(Entity.LeftPropertyPath))
                UpdateLeftSide(Entity.LeftPropertyPath);

            // Operator
            if (Entity.OperatorPluginGuid != null)
            {
                var conditionOperator = ConditionOperatorStore.Get(Entity.OperatorPluginGuid.Value, Entity.OperatorType)?.ConditionOperator;
                if (conditionOperator != null)
                    UpdateOperator(conditionOperator);
            }

            // Right side dynamic
            if (PredicateType == ProfileRightSideType.Dynamic && Entity.RightPropertyPath != null)
            {
                if (ListContainsInnerPath(Entity.RightPropertyPath))
                    UpdateRightSideDynamic(Entity.RightPropertyPath);
            }
            // Right side static
            else if (PredicateType == ProfileRightSideType.Static && Entity.RightStaticValue != null)
            {
                try
                {
                    if (LeftPropertyPath != null)
                    {
                        // Use the left side type so JSON.NET has a better idea what to do
                        var leftSideType = GetTypeAtInnerPath(LeftPropertyPath);
                        object rightSideValue;

                        try
                        {
                            rightSideValue = JsonConvert.DeserializeObject(Entity.RightStaticValue, leftSideType);
                        }
                        // If deserialization fails, use the type's default
                        catch (JsonSerializationException e)
                        {
                            DeserializationLogger.LogListPredicateDeserializationFailure(this, e);
                            rightSideValue = Activator.CreateInstance(leftSideType);
                        }

                        UpdateRightSideStatic(rightSideValue);
                    }
                    else
                    {
                        // Hope for the best... we must infer the type from JSON now
                        UpdateRightSideStatic(JsonConvert.DeserializeObject(Entity.RightStaticValue));
                    }
                }
                catch (JsonException e)
                {
                    DeserializationLogger.LogListPredicateDeserializationFailure(this, e);
                }
            }
        }

        internal override DisplayConditionPartEntity GetEntity()
        {
            return Entity;
        }

        private void CreateExpression()
        {
            CompiledListPredicate = null;

            if (Operator == null)
                return;

            // If the operator does not support a right side, create a static expression because the right side will simply be null
            if (PredicateType == ProfileRightSideType.Dynamic && Operator.SupportsRightSide)
                CreateDynamicExpression();

            CreateStaticExpression();
        }

        private void ValidateOperator()
        {
            if (LeftPropertyPath == null || Operator == null)
                return;

            var leftSideType = GetTypeAtInnerPath(LeftPropertyPath);
            if (!Operator.SupportsType(leftSideType))
                Operator = null;
        }

        private void ValidateRightSide()
        {
            var leftSideType = GetTypeAtInnerPath(LeftPropertyPath);
            if (PredicateType == ProfileRightSideType.Dynamic)
            {
                if (RightPropertyPath == null)
                    return;

                var rightSideType = GetTypeAtInnerPath(RightPropertyPath);
                if (!leftSideType.IsCastableFrom(rightSideType))
                    UpdateRightSideDynamic(null);
            }
            else
            {
                if (RightStaticValue != null && leftSideType.IsCastableFrom(RightStaticValue.GetType()))
                    UpdateRightSideStatic(RightStaticValue);
                else
                    UpdateRightSideStatic(null);
            }
        }

        private void SetStaticValue(object staticValue)
        {
            // If the left side is empty simply apply the value, any validation will wait
            if (LeftPropertyPath == null)
            {
                RightStaticValue = staticValue;
                return;
            }

            var leftSideType = GetTypeAtInnerPath(LeftPropertyPath);

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
            if (LeftPropertyPath == null || RightPropertyPath == null || Operator == null)
                return;

            // List accessors share the same parameter because a list always contains one item per entry
            var leftSideParameter = Expression.Parameter(typeof(object), "listItem");
            var leftSideAccessor = CreateListAccessor(LeftPropertyPath, leftSideParameter);
            var rightSideAccessor = CreateListAccessor(RightPropertyPath, leftSideParameter);

            // A conversion may be required if the types differ
            // This can cause issues if the DisplayConditionOperator wasn't accurate in it's supported types but that is not a concern here
            if (rightSideAccessor.Type != leftSideAccessor.Type)
                rightSideAccessor = Expression.Convert(rightSideAccessor, leftSideAccessor.Type);

            var conditionExpression = Operator.CreateExpression(leftSideAccessor, rightSideAccessor);
            var lambda = Expression.Lambda<Func<object, bool>>(conditionExpression, leftSideParameter);
            CompiledListPredicate = lambda.Compile();
        }

        private void CreateStaticExpression()
        {
            if (LeftPropertyPath == null || Operator == null)
                return;

            // List accessors share the same parameter because a list always contains one item per entry
            var leftSideParameter = Expression.Parameter(typeof(object), "listItem");
            var leftSideAccessor = CreateListAccessor(LeftPropertyPath, leftSideParameter);

            // If the left side is a value type but the input is empty, this isn't a valid expression
            if (leftSideAccessor.Type.IsValueType && RightStaticValue == null)
                return;

            // If the right side value is null, the constant type cannot be inferred and must be provided manually
            var rightSideConstant = RightStaticValue != null
                ? Expression.Constant(RightStaticValue)
                : Expression.Constant(null, leftSideAccessor.Type);

            var conditionExpression = Operator.CreateExpression(leftSideAccessor, rightSideConstant);
            var lambda = Expression.Lambda<Func<object, bool>>(conditionExpression, leftSideParameter);
            CompiledListPredicate = lambda.Compile();
        }

        private Expression CreateListAccessor(string path, ParameterExpression listParameter)
        {
            return path.Split('.').Aggregate<string, Expression>(
                Expression.Convert(listParameter, ListType), // Cast to the appropriate type
                Expression.Property
            );
        }

        #region Event handlers

        private void ConditionOperatorStoreOnConditionOperatorAdded(object sender, ConditionOperatorStoreEvent e)
        {
            var conditionOperator = e.Registration.ConditionOperator;
            if (Entity.OperatorPluginGuid == conditionOperator.PluginInfo.Guid && Entity.OperatorType == conditionOperator.GetType().Name)
                UpdateOperator(conditionOperator);
        }

        private void ConditionOperatorStoreOnConditionOperatorRemoved(object sender, ConditionOperatorStoreEvent e)
        {
            if (e.Registration.ConditionOperator != Operator)
                return;
            Operator = null;
            CompiledListPredicate = null;
        }

        #endregion

        #region IDisposable

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            ConditionOperatorStore.ConditionOperatorAdded -= ConditionOperatorStoreOnConditionOperatorAdded;
            ConditionOperatorStore.ConditionOperatorRemoved -= ConditionOperatorStoreOnConditionOperatorRemoved;

            base.Dispose(disposing);
        }

        #endregion
    }
}
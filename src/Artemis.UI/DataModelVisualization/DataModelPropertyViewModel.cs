using System;
using System.Linq.Expressions;
using System.Reflection;
using Artemis.Core.Plugins.Abstract.DataModels.Attributes;

namespace Artemis.UI.DataModelVisualization
{
    public class DataModelPropertyViewModel<T, TP> : DataModelPropertyViewModel
    {
        private readonly Func<T, TP> _expression;

        public DataModelPropertyViewModel(PropertyInfo propertyInfo, DataModelPropertyAttribute propertyDescription, DataModelViewModel parent)
        {
            PropertyInfo = propertyInfo;
            Parent = parent;
            PropertyDescription = propertyDescription;

            var instance = Expression.Parameter(typeof(T), "instance");
            var body = Expression.Property(instance, propertyInfo);
            _expression = Expression.Lambda<Func<T, TP>>(body, instance).Compile();
        }

        public TP Value
        {
            get => BaseValue is TP value ? value : default;
            set => BaseValue = value;
        }

        public override void Update()
        {
            Value = _expression((T) Parent.Model);
        }
    }

    public abstract class DataModelPropertyViewModel : DataModelVisualizationViewModel
    {
        public object BaseValue { get; protected set; }
    }
}
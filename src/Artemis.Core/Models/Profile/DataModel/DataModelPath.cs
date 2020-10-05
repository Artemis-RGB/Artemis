using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Artemis.Core.DataModelExpansions;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents a path that points to a property in data model
    /// </summary>
    public class DataModelPath
    {
        private Expression<Func<object, object>> _accessorLambda;
        private readonly LinkedList<DataModelPathSegment> _segments;

        /// <summary>
        ///     Creates a new instance of the <see cref="DataModelPath" /> class
        /// </summary>
        /// <param name="target">The target at which this path starts</param>
        /// <param name="path">A string representation of the <see cref="DataModelPath" /></param>
        public DataModelPath(object target, string path)
        {
            Target = target ?? throw new ArgumentNullException(nameof(target));
            Path = path ?? throw new ArgumentNullException(nameof(path));

            if (string.IsNullOrWhiteSpace(Path))
                throw new ArgumentException("Path cannot be empty");

            _segments = new LinkedList<DataModelPathSegment>();
            Initialize(path);
        }

        /// <summary>
        ///     Gets the data model at which this path starts
        /// </summary>
        public object Target { get; }

        /// <summary>
        ///     Gets a string representation of the <see cref="DataModelPath" />
        /// </summary>
        public string Path { get; }

        /// <summary>
        ///     Gets a boolean indicating whether all <see cref="Segments" /> are valid
        /// </summary>
        public bool IsValid => Segments.All(p => p.Type != DataModelPathSegmentType.Invalid);

        /// <summary>
        ///     Gets a read-only list of all segments of this path
        /// </summary>
        public IReadOnlyCollection<DataModelPathSegment> Segments => _segments.ToList().AsReadOnly();

        /// <summary>
        ///     Gets a boolean indicating whether this data model path points to a list
        /// </summary>
        public bool PointsToList => Segments.LastOrDefault()?.GetPropertyType() != null &&
                                    typeof(IList).IsAssignableFrom(Segments.LastOrDefault()?.GetPropertyType());

        internal Func<object, object> Accessor { get; private set; }

        /// <summary>
        ///     Gets the current value of the path
        /// </summary>
        public object GetValue()
        {
            if (_accessorLambda == null)
                return null;

            // If the accessor has not yet been compiled do it now that it's first required
            if (Accessor == null)
                Accessor = _accessorLambda.Compile();
            return Accessor(Target);
        }

        /// <summary>
        ///     Gets the property info of the property this path points to
        /// </summary>
        /// <returns>If static, the property info. If dynamic, <c>null</c></returns>
        public PropertyInfo GetPropertyInfo()
        {
            return Segments.LastOrDefault()?.GetPropertyInfo();
        }

        /// <summary>
        ///     Gets the type of the property this path points to
        /// </summary>
        /// <returns>If possible, the property type</returns>
        public Type GetPropertyType()
        {
            return Segments.LastOrDefault()?.GetPropertyType();
        }

        /// <summary>
        ///     Gets the property description of the property this path points to
        /// </summary>
        /// <returns>If found, the data model property description</returns>
        public DataModelPropertyAttribute GetPropertyDescription()
        {
            return Segments.LastOrDefault()?.GetPropertyDescription();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Path;
        }

        private void Initialize(string path)
        {
            DataModelPathSegment startSegment = new DataModelPathSegment(this, "target", "target");
            startSegment.Node = _segments.AddFirst(startSegment);

            string[] segments = path.Split(".");
            for (int index = 0; index < segments.Length; index++)
            {
                string identifier = segments[index];
                LinkedListNode<DataModelPathSegment> node = _segments.AddLast(new DataModelPathSegment(this, identifier, string.Join('.', segments.Take(index + 1))));
                node.Value.Node = node;
            }

            ParameterExpression parameter = Expression.Parameter(typeof(object), "t");
            Expression expression = Expression.Convert(parameter, Target.GetType());
            Expression nullCondition = null;

            foreach (DataModelPathSegment segment in _segments)
            {
                BinaryExpression notNull = Expression.NotEqual(expression, Expression.Default(expression.Type));
                nullCondition = nullCondition != null ? Expression.AndAlso(nullCondition, notNull) : notNull;
                expression = segment.Initialize(parameter, expression, nullCondition);
                if (expression == null)
                    return;
            }

            _accessorLambda = Expression.Lambda<Func<object, object>>(
                // Wrap with a null check
                Expression.Condition(
                    nullCondition,
                    Expression.Convert(expression, typeof(object)),
                    Expression.Convert(Expression.Default(expression.Type), typeof(object))
                ),
                parameter
            );
            Accessor = null;
        }
    }
}
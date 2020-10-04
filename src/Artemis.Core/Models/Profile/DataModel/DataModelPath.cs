using System;
using System.Collections;
using System.Collections.Generic;
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
        private readonly LinkedList<DataModelPathSegment> _segments;

        /// <summary>
        ///     Creates a new instance of the <see cref="DataModelPath" /> class
        /// </summary>
        /// <param name="dataModel">The data model at which this path starts</param>
        /// <param name="path">A string representation of the <see cref="DataModelPath" /></param>
        public DataModelPath(object dataModel, string path)
        {
            Target = dataModel ?? throw new ArgumentNullException(nameof(dataModel));
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
        ///     Gets a boolean indicating whether this data model path can have an inner path because it points to a list
        /// </summary>
        public bool CanHaveInnerPath => Segments.LastOrDefault()?.GetPropertyType()?.IsAssignableFrom(typeof(IList)) ?? false;

        /// <summary>
        ///     Gets the inner path of this path, only available if this path points to a list
        /// </summary>
        public DataModelPath InnerPath { get; internal set; }

        internal Func<object, object> Accessor { get; private set; }

        public void SetInnerPath(string path)
        {
            if (!CanHaveInnerPath)
            {
                var type = Segments.LastOrDefault()?.GetPropertyType();
                throw new ArtemisCoreException($"Cannot set an inner path on a data model path if it does not point to a list (value is of type: {type?.Name})");
            }

            InnerPath = new DataModelPath(GetValue(), path);
        }

        /// <summary>
        ///     Gets the current value of the path
        /// </summary>
        public object GetValue()
        {
            return Accessor?.Invoke(Target);
        }

        /// <summary>
        ///     Gets the property info of the property this path points to
        /// </summary>
        /// <returns>If static, the property info. If dynamic, <c>null</c></returns>
        public PropertyInfo GetPropertyInfo()
        {
            if (InnerPath != null)
                return InnerPath.GetPropertyInfo();
            return Segments.LastOrDefault()?.GetPropertyInfo();
        }

        /// <summary>
        ///     Gets the type of the property this path points to
        /// </summary>
        /// <returns>If possible, the property type</returns>
        public Type GetPropertyType()
        {
            if (InnerPath != null)
                return InnerPath.GetPropertyType();
            return Segments.LastOrDefault()?.GetPropertyType();
        }

        /// <summary>
        ///     Gets the property description of the property this path points to
        /// </summary>
        /// <returns>If found, the data model property description</returns>
        public DataModelPropertyAttribute GetPropertyDescription()
        {
            if (InnerPath != null)
                return InnerPath.GetPropertyDescription();
            return Segments.LastOrDefault()?.GetPropertyDescription();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            if (InnerPath != null)
                return $"{Path} > {InnerPath}";
            return Path;
        }

        private void Initialize(string path)
        {
            var segments = path.Split(".");
            for (var index = 0; index < segments.Length; index++)
            {
                var identifier = segments[index];
                var node = _segments.AddLast(new DataModelPathSegment(this, identifier, string.Join('.', segments.Take(index + 1))));
                node.Value.Node = node;
            }

            var parameter = Expression.Parameter(typeof(object), "t");
            Expression expression = Expression.Convert(parameter, Target.GetType());
            Expression nullCondition = null;

            foreach (var segment in _segments)
            {
                var notNull = Expression.NotEqual(expression, Expression.Default(expression.Type));
                nullCondition = nullCondition != null ? Expression.AndAlso(nullCondition, notNull) : notNull;
                expression = segment.Initialize(parameter, expression, nullCondition);
                if (expression == null)
                    return;
            }

            Accessor = Expression.Lambda<Func<object, object>>(
                // Wrap with a null check
                Expression.Condition(
                    nullCondition,
                    Expression.Convert(expression, typeof(object)),
                    Expression.Convert(Expression.Default(expression.Type), typeof(object))
                ),
                parameter
            ).Compile();
        }
    }
}
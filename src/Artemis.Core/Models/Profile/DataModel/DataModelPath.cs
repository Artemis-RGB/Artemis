using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Artemis.Core.DataModelExpansions;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents a path that points to a property in data model
    /// </summary>
    public class DataModelPath
    {
        private readonly LinkedList<DataModelPathPart> _parts;

        internal DataModelPath(DataModel dataModel, string path)
        {
            DataModel = dataModel ?? throw new ArgumentNullException(nameof(dataModel));
            Path = path ?? throw new ArgumentNullException(nameof(path));
            DataModelGuid = dataModel.PluginInfo.Guid;

            _parts = new LinkedList<DataModelPathPart>();
            Initialize(path);
        }

        /// <summary>
        ///     Gets the data model at which this path starts
        /// </summary>
        public DataModel DataModel { get; }

        /// <summary>
        ///     Gets a string representation of the <see cref="DataModelPath" />
        /// </summary>
        public string Path { get; }

        /// <summary>
        ///     Gets a boolean indicating whether all <see cref="Parts" /> are valid
        /// </summary>
        public bool IsValid => Parts.All(p => p.Type != DataModelPathPartType.Invalid);

        /// <summary>
        ///     Gets a read-only list of all parts of this path
        /// </summary>
        public IReadOnlyCollection<DataModelPathPart> Parts => _parts.ToList().AsReadOnly();

        internal Func<DataModel, object> Accessor { get; private set; }

        internal Guid DataModelGuid { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return Path;
        }

        private void Initialize(string path)
        {
            var parts = path.Split(".");
            for (var index = 0; index < parts.Length; index++)
            {
                var identifier = parts[index];
                var node = _parts.AddLast(new DataModelPathPart(this, identifier, string.Join('.', parts.Take(index + 1))));
                node.Value.Node = node;
            }

            var parameter = Expression.Parameter(typeof(DataModel), "dm");
            Expression expression = Expression.Convert(parameter, DataModel.GetType());
            Expression nullCondition = null;

            foreach (var part in _parts)
            {
                var notNull = Expression.NotEqual(expression, Expression.Constant(null));
                nullCondition = nullCondition != null ? Expression.AndAlso(nullCondition, notNull) : notNull;
                expression = part.Initialize(parameter, expression, nullCondition);
                if (expression == null)
                    return;
            }

            Accessor = Expression.Lambda<Func<DataModel, object>>(
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
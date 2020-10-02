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
        private readonly List<DataModelPathPart> _parts;

        // TODO: Make internal
        public DataModelPath(DataModel dataModel, string path)
        {
            if (dataModel == null)
                throw new ArgumentNullException(nameof(dataModel));
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            _parts = new List<DataModelPathPart>();

            DataModel = dataModel;
            DataModelGuid = dataModel.PluginInfo.Guid;

            Initialize(path);
        }

        private void Initialize(string path)
        {
            var parts = path.Split(".");
            foreach (var identifier in parts)
                _parts.Add(new DataModelPathPart(this, identifier));

            var parameter = Expression.Parameter(typeof(DataModel), "dm");
            Expression expression = Expression.Convert(parameter, DataModel.GetType());
            Expression nullCondition = null;
            DataModelPathPart previous = null;

            foreach (var part in _parts)
            {
                var notNull = Expression.NotEqual(expression, Expression.Constant(null));
                nullCondition = nullCondition != null ? Expression.AndAlso(nullCondition, notNull) : notNull;
                expression = part.Initialize(previous, parameter, expression, nullCondition);
                if (expression == null)
                    return;

                previous = part;
            }
        }

        /// <summary>
        ///     Gets the data model at which this path starts
        /// </summary>
        public DataModel DataModel { get; }

        /// <summary>
        ///     Gets a read-only list of all parts of this path
        /// </summary>
        public IReadOnlyCollection<DataModelPathPart> Parts => _parts.ToList().AsReadOnly();

        internal Guid DataModelGuid { get; set; }

        public string GetPathToPart(DataModelPathPart part)
        {
            var endIndex = _parts.IndexOf(part);
            return endIndex < 0 ? null : string.Join('.', _parts.Take(endIndex + 1));
        }

        internal DataModelPathPart GetPartBefore(DataModelPathPart dataModelPathPart)
        {
            var index = _parts.IndexOf(dataModelPathPart);
            return index > 0 ? _parts[index - 1] : null;
        }

        internal DataModelPathPart GetPartAfter(DataModelPathPart dataModelPathPart)
        {
            var index = _parts.IndexOf(dataModelPathPart);
            return index < _parts.Count - 1 ? _parts[index + 1] : null;
        }
    }
}
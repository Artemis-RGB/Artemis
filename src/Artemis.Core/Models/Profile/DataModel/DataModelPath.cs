using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.Core.DataModelExpansions;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents a path that points to a property in data model
    /// </summary>
    public class DataModelPath
    {
        private readonly LinkedList<DataModelPathPart> _parts;

        internal DataModelPath()
        {
            _parts = new LinkedList<DataModelPathPart>();
        }

        /// <summary>
        ///     Gets the data model at which this path starts
        /// </summary>
        public DataModel DataModel { get; private set; }

        /// <summary>
        ///     Gets a read-only list of all parts of this path
        /// </summary>
        public IReadOnlyCollection<DataModelPathPart> Parts => _parts.ToList().AsReadOnly();

        internal Guid DataModelGuid { get; set; }
    }
}
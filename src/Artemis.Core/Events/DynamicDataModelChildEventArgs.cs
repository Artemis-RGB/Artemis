using System;
using Artemis.Core.DataModelExpansions;

namespace Artemis.Core
{
    /// <summary>
    ///     Provides data about dynamic data model child related events
    /// </summary>
    public class DynamicDataModelChildEventArgs : EventArgs
    {
        internal DynamicDataModelChildEventArgs(object? dynamicChild, string key)
        {
            DynamicChild = dynamicChild;
            Key = key;
        }

        /// <summary>
        ///     Gets the dynamic data model child
        /// </summary>
        public object? DynamicChild { get; }

        /// <summary>
        ///     Gets the key of the dynamic data model on the parent <see cref="DataModel" />
        /// </summary>
        public string Key { get; }
    }
}
using System;
using Artemis.Core.DataModelExpansions;

namespace Artemis.Core
{
    /// <summary>
    ///     Provides data about dynamic data model related events
    /// </summary>
    public class DynamicDataModelEventArgs : EventArgs
    {
        internal DynamicDataModelEventArgs(DataModel dynamicDataModel, string key)
        {
            DynamicDataModel = dynamicDataModel;
            Key = key;
        }

        /// <summary>
        ///     Gets the dynamic data model
        /// </summary>
        public DataModel DynamicDataModel { get; }

        /// <summary>
        ///     Gets the key of the dynamic data model on the parent <see cref="DataModel" />
        /// </summary>
        public string Key { get; }
    }
}
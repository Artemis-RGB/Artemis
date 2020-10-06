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

        public DataModel DynamicDataModel { get; }
        public string Key { get; }
    }
}
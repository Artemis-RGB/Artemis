using System;

namespace Artemis.Core
{
    /// <summary>
    ///     Provides data about data model path related events
    /// </summary>
    public class DataModelPathEventArgs : EventArgs
    {
        internal DataModelPathEventArgs(DataModelPath dataModelPath)
        {
            DataModelPath = dataModelPath;
        }

        /// <summary>
        ///     Gets the data model path this event is related to
        /// </summary>
        public DataModelPath DataModelPath { get; }
    }
}
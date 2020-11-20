using System;
using Artemis.Core;
using Artemis.UI.Shared.Input;

namespace Artemis.UI.Shared
{
    /// <summary>
    ///     Provides data about selection events raised by <see cref="DataModelDynamicViewModel" />
    /// </summary>
    public class DataModelInputDynamicEventArgs : EventArgs
    {
        internal DataModelInputDynamicEventArgs(DataModelPath dataModelPath)
        {
            DataModelPath = dataModelPath;
        }

        /// <summary>
        ///     Gets the data model path that was selected
        /// </summary>
        public DataModelPath DataModelPath { get; }
    }
}
using System;
using Artemis.Core;

namespace Artemis.UI.Avalonia.Shared.Events
{
    /// <summary>
    ///     Provides data about selection events raised by <see cref="DataModelDynamicViewModel" />
    /// </summary>
    public class DataModelInputDynamicEventArgs : EventArgs
    {
        internal DataModelInputDynamicEventArgs(DataModelPath? dataModelPath)
        {
            DataModelPath = dataModelPath;
        }

        /// <summary>
        ///     Gets the data model path that was selected
        /// </summary>
        public DataModelPath? DataModelPath { get; }
    }
}
using System;
using Artemis.Core;
using Artemis.UI.Shared.Controls;

namespace Artemis.UI.Shared.Events
{
    /// <summary>
    ///     Provides data about selection events raised by <see cref="DataModelPicker" />
    /// </summary>
    public class DataModelSelectedEventArgs : EventArgs
    {
        /// <summary>
        ///     Gets the data model path that was selected
        /// </summary>
        public DataModelPath? Path { get; }

        internal DataModelSelectedEventArgs(DataModelPath? path)
        {
            Path = path;
        }
    }
}
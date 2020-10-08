using System;
using Artemis.Core;

namespace Artemis.UI.Shared
{
    public class DataModelInputDynamicEventArgs : EventArgs
    {
        public DataModelPath DataModelPath { get; }

        public DataModelInputDynamicEventArgs(DataModelPath dataModelPath)
        {
            DataModelPath = dataModelPath;
        }
    }
}
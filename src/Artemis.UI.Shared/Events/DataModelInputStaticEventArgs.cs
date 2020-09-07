using System;

namespace Artemis.UI.Shared
{
    public class DataModelInputStaticEventArgs : EventArgs
    {
        public object Value { get; }

        public DataModelInputStaticEventArgs(object value)
        {
            Value = value;
        }
    }
}
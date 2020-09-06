using System;

namespace Artemis.Core
{
    public class DataBindingPropertyUpdatedEvent<T> : EventArgs
    {
        public DataBindingPropertyUpdatedEvent(T value)
        {
            Value = value;
        }

        /// <summary>
        ///     The updated value that should be applied to the layer property
        /// </summary>
        public T Value { get; }
    }
}
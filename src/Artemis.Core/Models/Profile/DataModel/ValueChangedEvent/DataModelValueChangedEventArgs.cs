using Artemis.Core.DataModelExpansions;

namespace Artemis.Core
{
    internal class DataModelValueChangedEventArgs<T> : DataModelEventArgs
    {
        public DataModelValueChangedEventArgs(T? currentValue, T? previousValue)
        {
            CurrentValue = currentValue;
            PreviousValue = previousValue;
        }

        [DataModelProperty(Description = "The current value of the property")]
        public T? CurrentValue { get; }
        [DataModelProperty(Description = "The previous value of the property")]
        public T? PreviousValue { get; }
    }
}
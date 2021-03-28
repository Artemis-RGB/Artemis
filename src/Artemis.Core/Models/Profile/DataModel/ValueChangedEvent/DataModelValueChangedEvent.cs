using System;
using System.Collections.Generic;

namespace Artemis.Core
{
    internal class DataModelValueChangedEvent<T> : IDataModelEvent
    {
        public DataModelValueChangedEvent(DataModelPath path)
        {
            Path = path;
        }

        public DataModelPath Path { get; }
        public T? LastValue { get; private set; }
        public T? CurrentValue { get; private set; }
        public DateTime LastTrigger { get; private set; }
        public TimeSpan TimeSinceLastTrigger => DateTime.Now - LastTrigger;
        public int TriggerCount { get; private set; }
        public Type ArgumentsType { get; } = typeof(DataModelValueChangedEventArgs<T>);
        public string TriggerPastParticiple => "changed";
        public bool TrackHistory { get; set; } = false;
        public DataModelEventArgs? LastEventArgumentsUntyped { get; private set; }
        public List<DataModelEventArgs> EventArgumentsHistoryUntyped { get; } = new();

        public void Update()
        {
            object? value = Path.GetValue();
            if (value != null)
                CurrentValue = (T?) value;
            else
                CurrentValue = default;

            if (!Equals(LastValue, CurrentValue))
                Trigger();

            LastValue = CurrentValue;
        }

        public void Reset()
        {
            TriggerCount = 0;
        }

        private void Trigger()
        {
            LastEventArgumentsUntyped = new DataModelValueChangedEventArgs<T>(CurrentValue, LastValue);
            LastTrigger = DateTime.Now;
            TriggerCount++;

            OnEventTriggered();
        }

        #region Events

        public event EventHandler? EventTriggered;

        internal virtual void OnEventTriggered()
        {
            EventTriggered?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}
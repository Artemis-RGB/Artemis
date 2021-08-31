using System;
using Newtonsoft.Json;

namespace Artemis.Core
{
    public sealed class InputPin<T> : Pin
    {
        #region Properties & Fields

        public override Type Type { get; } = typeof(T);
        public override object PinValue => Value;
        public override PinDirection Direction => PinDirection.Input;

        private T _value;
        public T Value
        {
            get
            {
                if (!IsEvaluated)
                    Evaluate();

                return _value;
            }

            private set
            {
                _value = value;
                IsEvaluated = true;
                OnPropertyChanged(nameof(PinValue));
            }
        }

        #endregion

        #region Constructors

        [JsonConstructor]
        internal InputPin(INode node, string name)
            : base(node, name)
        { }

        #endregion

        #region Methods

        private void Evaluate()
        {
            if (ConnectedTo.Count > 0)
                if (ConnectedTo[0].PinValue is T value)
                    Value = value;
        }

        #endregion
    }

    public sealed class InputPin : Pin
    {
        #region Properties & Fields

        public override Type Type { get; }
        public override object PinValue => Value;
        public override PinDirection Direction => PinDirection.Input;

        private object _value;
        public object Value
        {
            get
            {
                if (!IsEvaluated)
                    Evaluate();

                return _value;
            }

            private set
            {
                if (!Type.IsInstanceOfType(value)) throw new ArgumentException($"Value of type '{value?.GetType().Name ?? "null"}' can't be assigned to a pin of type {Type.Name}.");

                _value = value;
                IsEvaluated = true;
                OnPropertyChanged(nameof(PinValue));
            }
        }

        #endregion

        #region Constructors

        internal InputPin(INode node, Type type, string name)
            : base(node, name)
        {
            this.Type = type;
        }

        #endregion

        #region Methods

        private void Evaluate()
        {
            Value = ConnectedTo.Count > 0 ? ConnectedTo[0].PinValue : Type.GetDefault();
        }

        #endregion
    }
}

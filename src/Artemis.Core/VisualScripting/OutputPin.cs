using System;
using Newtonsoft.Json;

namespace Artemis.Core
{
    public sealed class OutputPin<T> : Pin
    {
        #region Properties & Fields

        public override Type Type { get; } = typeof(T);
        public override object PinValue => Value;
        public override PinDirection Direction => PinDirection.Output;

        private T _value;
        public T Value
        {
            get
            {
                if (!IsEvaluated)
                    Node?.Evaluate();

                return _value;
            }
            set
            {
                _value = value;
                IsEvaluated = true;
            }
        }

        #endregion

        #region Constructors

        [JsonConstructor]
        internal OutputPin(INode node, string name)
            : base(node, name)
        { }

        #endregion
    }

    public sealed class OutputPin : Pin
    {
        #region Properties & Fields

        public override Type Type { get; }
        public override object PinValue => Value;
        public override PinDirection Direction => PinDirection.Output;

        private object _value;
        public object Value
        {
            get
            {
                if (!IsEvaluated)
                    Node?.Evaluate();

                return _value;
            }
            set
            {
                if (!Type.IsInstanceOfType(value)) throw new ArgumentException($"Value of type '{value?.GetType().Name ?? "null"}' can't be assigned to a pin of type {Type.Name}.");

                _value = value;
                IsEvaluated = true;
            }
        }

        #endregion

        #region Constructors

        internal OutputPin(INode node, Type type, string name)
            : base(node, name)
        {
            this.Type = type;
        }

        #endregion
    }
}

using System;
using Artemis.Core.VisualScripting;

namespace Artemis.VisualScripting.Model
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
            }
        }

        #endregion

        #region Constructors

        internal InputPin(INode node, string name)
            : base(node, name)
        { }

        #endregion

        #region Methods
        
        private void Evaluate()
        {
            Value = ConnectedTo.Count > 0 ? (T)ConnectedTo[0].PinValue : default;
        }

        #endregion
    }
}

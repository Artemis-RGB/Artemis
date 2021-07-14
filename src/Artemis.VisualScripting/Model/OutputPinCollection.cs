using System;
using Artemis.Core.VisualScripting;

namespace Artemis.VisualScripting.Model
{
    public sealed class OutputPinCollection<T> : PinCollection
    {
        #region Properties & Fields

        public override PinDirection Direction => PinDirection.Output;
        public override Type Type => typeof(T);

        #endregion

        #region Constructors

        internal OutputPinCollection(INode node, string name, int initialCount)
            : base(node, name, initialCount)
        { }

        #endregion

        #region Methods

        protected override IPin CreatePin() => new OutputPin<T>(Node, string.Empty);

        #endregion
    }
}

using System;
using Artemis.VisualScripting.Model;

namespace Artemis.VisualScripting.Internal
{
    internal class IsConnectingPin : Pin
    {
        #region Properties & Fields

        public override PinDirection Direction { get; }
        public override Type Type { get; } = typeof(object);
        public override object PinValue => null;

        #endregion

        #region Constructors

        public IsConnectingPin(PinDirection direction)
            : base(null, null)
        {
            this.Direction = direction;
        }

        #endregion
    }
}

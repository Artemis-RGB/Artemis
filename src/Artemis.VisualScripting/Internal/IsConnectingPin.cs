using System;
using Artemis.Core;

namespace Artemis.VisualScripting.Internal
{
    internal class IsConnectingPin : Pin
    {
        #region Properties & Fields

        public override PinDirection Direction { get; }
        public override Type Type { get; }
        public override object PinValue => null;

        #endregion

        #region Constructors

        public IsConnectingPin(PinDirection direction, Type type)
            : base(null, null)
        {
            this.Direction = direction;
            this.Type = type;
        }

        #endregion
    }
}

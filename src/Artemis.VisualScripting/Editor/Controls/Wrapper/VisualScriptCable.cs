using System;
using Artemis.Core.VisualScripting;
using Artemis.VisualScripting.Model;
using Artemis.VisualScripting.ViewModel;

namespace Artemis.VisualScripting.Editor.Controls.Wrapper
{
    public class VisualScriptCable : AbstractBindable
    {
        #region Properties & Fields

        private VisualScriptPin _from;
        public VisualScriptPin From
        {
            get => _from;
            private set => SetProperty(ref _from, value);
        }

        private VisualScriptPin _to;
        public VisualScriptPin To
        {
            get => _to;
            private set => SetProperty(ref _to, value);
        }

        #endregion

        #region Constructors

        public VisualScriptCable(VisualScriptPin pin1, VisualScriptPin pin2)
        {
            if ((pin1.Pin.Direction == PinDirection.Input) && (pin2.Pin.Direction == PinDirection.Input))
                throw new ArgumentException("Can't connect two input pins.");

            if ((pin1.Pin.Direction == PinDirection.Output) && (pin2.Pin.Direction == PinDirection.Output))
                throw new ArgumentException("Can't connect two output pins.");

            From = pin1.Pin.Direction == PinDirection.Output ? pin1 : pin2;
            To = pin1.Pin.Direction == PinDirection.Input ? pin1 : pin2;

            From.ConnectTo(this);
            To.ConnectTo(this);
        }

        #endregion

        #region Methods

        internal void Disconnect()
        {
            From?.Disconnect(this);
            To?.Disconnect(this);

            From = null;
            To = null;
        }

        internal IPin GetConnectedPin(IPin pin)
        {
            if (From.Pin == pin) return To.Pin;
            if (To.Pin == pin) return From.Pin;
            return null;
        }

        #endregion
    }
}

using System;
using Artemis.VisualScripting.Editor.Controls.Wrapper;

namespace Artemis.VisualScripting.Events
{
    public class PinDisconnectedEventArgs : EventArgs
    {
        #region Properties & Fields

        public VisualScriptPin Pin { get; }
        public VisualScriptCable Cable { get; }

        #endregion

        #region Constructors

        public PinDisconnectedEventArgs(VisualScriptPin pin, VisualScriptCable cable)
        {
            this.Pin = pin;
            this.Cable = cable;
        }

        #endregion
    }
}

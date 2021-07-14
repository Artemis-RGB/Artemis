using System;
using Artemis.VisualScripting.Editor.Controls.Wrapper;

namespace Artemis.VisualScripting.Events
{
    public class PinConnectedEventArgs : EventArgs
    {
        #region Properties & Fields

        public VisualScriptPin Pin { get; }
        public VisualScriptCable Cable { get; }

        #endregion

        #region Constructors

        public PinConnectedEventArgs(VisualScriptPin pin, VisualScriptCable cable)
        {
            this.Pin = pin;
            this.Cable = cable;
        }

        #endregion
    }
}

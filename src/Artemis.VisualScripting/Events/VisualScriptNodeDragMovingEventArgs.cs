using System;

namespace Artemis.VisualScripting.Events
{
    public class VisualScriptNodeDragMovingEventArgs : EventArgs
    {
        #region Properties & Fields

        public double DX { get; }
        public double DY { get; }

        #endregion

        #region Constructors

        public VisualScriptNodeDragMovingEventArgs(double dx, double dy)
        {
            this.DX = dx;
            this.DY = dy;
        }

        #endregion
    }
}

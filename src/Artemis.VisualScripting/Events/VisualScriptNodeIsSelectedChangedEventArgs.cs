using System;

namespace Artemis.VisualScripting.Events
{
    public class VisualScriptNodeIsSelectedChangedEventArgs : EventArgs
    {
        #region Properties & Fields

        public bool IsSelected { get; }
        public bool AlterSelection { get; }

        #endregion

        #region Constructors

        public VisualScriptNodeIsSelectedChangedEventArgs(bool isSelected, bool alterSelection)
        {
            this.IsSelected = isSelected;
            this.AlterSelection = alterSelection;
        }

        #endregion
    }
}

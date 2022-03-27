using System;

namespace Artemis.UI.Shared.Events
{
    internal class DialogClosedEventArgs<TResult> : EventArgs
    {
        public TResult Result { get; }

        public DialogClosedEventArgs(TResult result)
        {
            Result = result;
        }
    }
}

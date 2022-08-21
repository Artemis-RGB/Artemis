using System;

namespace Artemis.UI.Shared.Events;

internal class DialogClosedEventArgs<TResult> : EventArgs
{
    public DialogClosedEventArgs(TResult result)
    {
        Result = result;
    }

    public TResult Result { get; }
}
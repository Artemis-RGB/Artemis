using System;

namespace Artemis.Core;

/// <summary>
///     Provides data for data binding events.
/// </summary>
public class DataBindingEventArgs : EventArgs
{
    internal DataBindingEventArgs(IDataBinding dataBinding)
    {
        DataBinding = dataBinding;
    }

    /// <summary>
    ///     Gets the data binding this event is related to
    /// </summary>
    public IDataBinding DataBinding { get; }
}
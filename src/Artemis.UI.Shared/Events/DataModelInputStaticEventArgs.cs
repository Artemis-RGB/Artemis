﻿using System;
using Artemis.UI.Shared.Input;

namespace Artemis.UI.Shared
{
    /// <summary>
    ///     Provides data about submit events raised by <see cref="DataModelStaticViewModel" />
    /// </summary>
    public class DataModelInputStaticEventArgs : EventArgs
    {
        internal DataModelInputStaticEventArgs(object value)
        {
            Value = value;
        }

        /// <summary>
        ///     The value that was submitted
        /// </summary>
        public object Value { get; }
    }
}
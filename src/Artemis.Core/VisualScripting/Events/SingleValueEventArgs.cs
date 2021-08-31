using System;

namespace Artemis.Core.Events
{
    public class SingleValueEventArgs<T> : EventArgs
    {
        #region Properties & Fields

        public T Value { get; }

        #endregion

        #region Constructors

        public SingleValueEventArgs(T value)
        {
            this.Value = value;
        }

        #endregion
    }
}

using System;
using System.Collections.Generic;
using Artemis.Core.Events;

namespace Artemis.Core
{
    public interface IPinCollection : IEnumerable<IPin>
    {
        string Name { get; }
        PinDirection Direction { get; }
        Type Type { get; }

        event EventHandler<SingleValueEventArgs<IPin>> PinAdded;
        event EventHandler<SingleValueEventArgs<IPin>> PinRemoved;

        IPin AddPin();
        bool Remove(IPin pin);
    }
}

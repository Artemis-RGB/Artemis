using System;
using System.Collections.Generic;

namespace Artemis.Core
{
    public interface IPinCollection : IEnumerable<IPin>
    {
        string Name { get; }
        PinDirection Direction { get; }
        Type Type { get; }

        event EventHandler<IPin> PinAdded;
        event EventHandler<IPin> PinRemoved;

        IPin AddPin();
        bool Remove(IPin pin);
    }
}

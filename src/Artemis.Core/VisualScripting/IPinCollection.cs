using System;
using System.Collections.Generic;
using Artemis.VisualScripting.Model;

namespace Artemis.Core.VisualScripting
{
    public interface IPinCollection : IEnumerable<IPin>
    {
        string Name { get; }
        PinDirection Direction { get; }
        Type Type { get; }

        IPin AddPin();
        bool Remove(IPin pin);
    }
}

using System;
using System.Collections.Generic;

namespace Artemis.Core
{
    public interface IPin
    {
        INode Node { get; }

        string Name { get; }
        PinDirection Direction { get; }
        Type Type { get; }
        object PinValue { get; }

        IReadOnlyList<IPin> ConnectedTo { get; }

        bool IsEvaluated { get; set; }

        event EventHandler<IPin> PinConnected;
        event EventHandler<IPin> PinDisconnected;

        void ConnectTo(IPin pin);
        void DisconnectFrom(IPin pin);
    }
}

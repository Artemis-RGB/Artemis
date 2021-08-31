using System;
using System.Collections.Generic;
using Artemis.Core.Events;

namespace Artemis.Core
{
    public interface IPin
    {
        INode Node { get; }

        string Name { get; set; }
        PinDirection Direction { get; }
        Type Type { get; }
        object PinValue { get; }

        IReadOnlyList<IPin> ConnectedTo { get; }

        bool IsEvaluated { get; set; }

        event EventHandler<SingleValueEventArgs<IPin>> PinConnected;
        event EventHandler<SingleValueEventArgs<IPin>> PinDisconnected;

        void ConnectTo(IPin pin);
        void DisconnectFrom(IPin pin);
    }
}

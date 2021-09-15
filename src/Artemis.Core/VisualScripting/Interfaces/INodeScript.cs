using System;
using System.Collections.Generic;
using System.ComponentModel;
using Artemis.Core.Events;

namespace Artemis.Core
{
    public interface INodeScript : INotifyPropertyChanged, IDisposable
    {
        string Name { get; }
        string Description { get; }
        
        IEnumerable<INode> Nodes { get; }

        Type ResultType { get; }

        object? Context { get; set; }
        
        event EventHandler<SingleValueEventArgs<INode>>? NodeAdded;
        event EventHandler<SingleValueEventArgs<INode>>? NodeRemoved;
        
        void Run();
        void AddNode(INode node);
        void RemoveNode(INode node);
    }

    public interface INodeScript<out T> : INodeScript
    {
        T Result { get; }
    }
}

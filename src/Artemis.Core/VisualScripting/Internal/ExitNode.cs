using System;

namespace Artemis.Core.Internal
{
    internal interface IExitNode : INode
    {
        protected static readonly Guid NodeId = new("410C824D-C5E3-4E3A-8080-D50F6C8B83B8");
    }

    internal class ExitNode<T> : Node, IExitNode
    {
        #region Properties & Fields
        
        public InputPin<T> Input { get; }

        public T? Value { get; private set; }

        public override bool IsExitNode => true;
        
        #endregion

        #region Constructors

        public ExitNode(string name, string description = "")
        {
            Id = IExitNode.NodeId;
            Name = name;
            Description = description;

            Input = CreateInputPin<T>();
        }

        #endregion

        #region Methods

        public override void Evaluate()
        {
            Value = Input.Value;
        }

        #endregion
    }
}

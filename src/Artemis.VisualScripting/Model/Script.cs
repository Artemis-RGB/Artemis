using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Artemis.Core.VisualScripting;
using Artemis.VisualScripting.Internal;
using Artemis.VisualScripting.ViewModel;
using JetBrains.Annotations;

namespace Artemis.VisualScripting.Model
{
    public abstract class Script : AbstractBindable, IScript
    {
        #region Properties & Fields

        public string Name { get; }
        public string Description { get; }

        private readonly List<INode> _nodes = new();
        public IEnumerable<INode> Nodes => new ReadOnlyCollection<INode>(_nodes);

        protected INode ExitNode { get; set; }
        public abstract Type ResultType { get; }

        #endregion

        #region Constructors

        public Script(string name, string description)
        {
            this.Name = name;
            this.Description = description;
        }

        #endregion

        #region Methods

        public void Run()
        {
            foreach (INode node in Nodes)
                node.Reset();

            ExitNode.Evaluate();
        }

        public void AddNode(INode node)
        {
            _nodes.Add(node);
        }

        public void RemoveNode(INode node)
        {
            _nodes.Remove(node);
        }
        
        public void Dispose()
        { }

        #endregion
    }

    public class Script<T> : Script, IScript<T>
    {
        #region Properties & Fields

        public T Result => ((ExitNode<T>)ExitNode).Value;

        public override Type ResultType => typeof(T);

        #endregion

        #region Constructors

        public Script(string name, string description)
            : base(name, description)
        {
            ExitNode = new ExitNode<T>(name, description);
            AddNode(ExitNode);
        }

        [UsedImplicitly(ImplicitUseKindFlags.InstantiatedWithFixedConstructorSignature)]
        private Script(string name, string description, INode exitNode)
            : base(name, description)
        {
            ExitNode = exitNode;
            AddNode(ExitNode);
        }

        #endregion
    }
}

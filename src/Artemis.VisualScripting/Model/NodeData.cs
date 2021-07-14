using System;
using Artemis.Core.VisualScripting;

namespace Artemis.VisualScripting.Model
{
    public class NodeData
    {
        #region Properties & Fields

        public Type Type { get; }
        public string Name { get; }
        public string Description { get; }
        public string Category { get; }

        private Func<INode> _create;

        #endregion

        #region Constructors

        public NodeData(Type type, string name, string description, string category, Func<INode> create)
        {
            this.Type = type;
            this.Name = name;
            this.Description = description;
            this.Category = category;
            this._create = create;
        }

        #endregion

        #region Methods

        public INode CreateNode() => _create();

        #endregion
    }
}

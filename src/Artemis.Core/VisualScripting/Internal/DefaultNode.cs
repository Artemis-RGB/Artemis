using System;

namespace Artemis.Core.Internal
{
    /// <summary>
    ///     Represents a kind of node that cannot be deleted inside a <see cref="INode" />.
    /// </summary>
    public interface IDefaultNode : INode
    {
    }

    /// <summary>
    ///     Represents a kind of node that cannot be deleted inside a <see cref="NodeScript" />.
    /// </summary>
    public abstract class DefaultNode : Node, IDefaultNode
    {
        #region Properties & Fields

        /// <inheritdoc />
        public override bool IsDefaultNode => true;

        #endregion

        #region Constructors

        /// <inheritdoc />
        protected DefaultNode(Guid id, string name, string description = "") : base(name, description)
        {
            Id = id;
            Name = name;
            Description = description;
        }

        #endregion
    }
}
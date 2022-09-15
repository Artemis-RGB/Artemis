using System;

namespace Artemis.Core;

/// <summary>
///     Represents a kind of node that cannot be deleted inside a <see cref="NodeScript" />.
/// </summary>
public abstract class DefaultNode : Node
{
    #region Constructors

    /// <inheritdoc />
    protected DefaultNode(Guid id, string name, string description = "") : base(name, description)
    {
        Id = id;
    }

    #endregion

    #region Properties & Fields

    /// <inheritdoc />
    public override bool IsDefaultNode => true;

    #endregion
}
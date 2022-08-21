using System;
using Artemis.Core;

namespace Artemis.UI.Shared.Services.NodeEditor.Commands;

/// <summary>
///     Represents a node editor command that can be used to update the storage value of a node.
/// </summary>
/// <typeparam name="TStorage">The type of value the node stores</typeparam>
public class UpdateStorage<TStorage> : INodeEditorCommand, IDisposable
{
    private readonly Node<TStorage> _node;
    private readonly TStorage? _originalValue;
    private readonly TStorage? _value;
    private readonly string _valueDescription;
    private bool _updatedValue;

    /// <summary>
    ///     Creates a new instance of the <see cref="UpdateStorage{T}" /> class.
    /// </summary>
    /// <param name="node">The node to update.</param>
    /// <param name="value">The new value of the node.</param>
    /// <param name="valueDescription">The description of the value that was updated.</param>
    public UpdateStorage(Node<TStorage> node, TStorage? value, string valueDescription = "value")
    {
        _node = node;
        _value = value;
        _valueDescription = valueDescription;

        _originalValue = _node.Storage;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_updatedValue)
        {
            if (_originalValue is IDisposable disposable)
                disposable.Dispose();
        }
        else
        {
            if (_value is IDisposable disposable)
                disposable.Dispose();
        }
    }

    #region Implementation of INodeEditorCommand

    /// <inheritdoc />
    public string DisplayName => $"Update node {_valueDescription}";

    /// <inheritdoc />
    public void Execute()
    {
        _updatedValue = true;
        _node.Storage = _value;
    }

    /// <inheritdoc />
    public void Undo()
    {
        _updatedValue = false;
        _node.Storage = _originalValue;
    }

    #endregion
}
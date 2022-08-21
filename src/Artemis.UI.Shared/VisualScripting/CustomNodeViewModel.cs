using System;
using System.ComponentModel;
using System.Reactive.Disposables;
using Artemis.Core;
using ReactiveUI;
using ReactiveUI.Validation.Helpers;

namespace Artemis.UI.Shared.VisualScripting;

/// <summary>
///     Represents a custom view model for a node
/// </summary>
public abstract class CustomNodeViewModel : ReactiveValidationObject, IActivatableViewModel, ICustomNodeViewModel
{
    /// <summary>
    ///     Creates a new instance of the <see cref="CustomNodeViewModel" /> class.
    /// </summary>
    /// <param name="node">The node the view model is for.</param>
    /// <param name="script">The script the node is contained in.</param>
    protected CustomNodeViewModel(INode node, INodeScript script)
    {
        Node = node;
        Script = script;

        this.WhenActivated(d =>
        {
            Node.PropertyChanged += NodeOnPropertyChanged;
            Disposable.Create(() => Node.PropertyChanged -= NodeOnPropertyChanged).DisposeWith(d);
        });
    }

    /// <summary>
    ///     Gets script the node is contained in.
    /// </summary>
    public INodeScript Script { get; }

    /// <summary>
    ///     Invokes the <see cref="NodeModified" /> event
    /// </summary>
    protected virtual void OnNodeModified()
    {
        NodeModified?.Invoke(this, EventArgs.Empty);
    }

    private void NodeOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "Storage")
            OnNodeModified();
    }

    #region Implementation of IActivatableViewModel

    /// <inheritdoc />
    public ViewModelActivator Activator { get; } = new();

    #endregion

    /// <summary>
    ///     Gets the node the view model is for.
    /// </summary>
    public INode Node { get; }

    /// <inheritdoc />
    public event EventHandler? NodeModified;
}
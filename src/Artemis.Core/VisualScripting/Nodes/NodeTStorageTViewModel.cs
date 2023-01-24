using System.Linq;
using System.Reflection;
using Artemis.Core;
using DryIoc;

/// <summary>
///     Represents a kind of node inside a <see cref="NodeScript" /> containing storage value of type
///     <typeparamref name="TStorage" /> and a view model of type <typeparamref name="TViewModel" />.
/// </summary>
/// <typeparam name="TStorage">The type of value the node stores</typeparam>
/// <typeparam name="TViewModel">The type of view model the node uses</typeparam>
public abstract class Node<TStorage, TViewModel> : Node<TStorage>, ICustomViewModelNode where TViewModel : ICustomNodeViewModel
{
    /// <inheritdoc />
    protected Node()
    {
    }

    /// <inheritdoc />
    protected Node(string name, string description) : base(name, description)
    {
    }

    /// <summary>
    ///     Called when a view model is required
    /// </summary>
    /// <param name="nodeScript"></param>
    public virtual TViewModel GetViewModel(NodeScript nodeScript)
    {
        // Limit to one constructor, there's no need to have more and it complicates things anyway
        ConstructorInfo[] constructors = typeof(TViewModel).GetConstructors();
        if (constructors.Length != 1)
            throw new ArtemisCoreException("Node VMs must have exactly one constructor");

        // Find the ScriptConfiguration parameter, it is required by the base constructor so its there for sure
        ParameterInfo? configurationParameter = constructors.First().GetParameters().FirstOrDefault(p => GetType().IsAssignableFrom(p.ParameterType));

        if (configurationParameter?.Name == null)
            throw new ArtemisCoreException($"Couldn't find a valid constructor argument on {typeof(TViewModel).Name} with type {GetType().Name}");
        return Container.Resolve<TViewModel>(args: new object[] {this, nodeScript});
    }

    /// <summary>
    ///     Gets or sets the position of the node's custom view model.
    /// </summary>
    public CustomNodeViewModelPosition ViewModelPosition { get; protected set; } = CustomNodeViewModelPosition.BetweenPinsTop;

    /// <param name="nodeScript"></param>
    /// <inheritdoc />
    public ICustomNodeViewModel GetCustomViewModel(NodeScript nodeScript)
    {
        return GetViewModel(nodeScript);
    }
}
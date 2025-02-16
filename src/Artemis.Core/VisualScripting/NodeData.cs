using System;
using Artemis.Core.Nodes;
using Artemis.Storage.Entities.Profile.Nodes;

namespace Artemis.Core;

/// <summary>
///     Represents node data describing a certain <see cref="INode" />
/// </summary>
public class NodeData
{
    #region Constructors

    internal NodeData(NodeProvider provider, Type type, string name, string description, string category, string helpUrl, Type? inputType, Type? outputType)
    {
        Provider = provider;
        Type = type;
        Name = name;
        Description = description;
        Category = category;
        HelpUrl = helpUrl;
        InputType = inputType;
        OutputType = outputType;
    }

    #endregion

    #region Methods

    /// <summary>
    ///     Creates a new instance of the node this data represents
    /// </summary>
    /// <param name="script">The script to create the node for</param>
    /// <param name="entity">An optional storage entity to apply to the node</param>
    /// <returns>The returning node of type <see cref="Type" /></returns>
    public INode CreateNode(INodeScript script, NodeEntity? entity)
    {
        INode node = (INode) Provider.Plugin.Resolve(Type);
        node.NodeData = this;
        if (string.IsNullOrWhiteSpace(node.Name))
            node.Name = Name;
        if (string.IsNullOrWhiteSpace(node.Description))
            node.Description = Description;
        if (string.IsNullOrWhiteSpace(node.HelpUrl))
            node.HelpUrl = HelpUrl;
        
        if (entity != null)
        {
            node.IsLoading = true;
            node.X = entity.X;
            node.Y = entity.Y;
            try
            {
                if (node is Node nodeImplementation)
                    nodeImplementation.DeserializeStorage(entity.Storage);
            }
            catch
            {
                // ignored
            }
        }

        node.TryInitialize(script);
        return node;
    }

    #endregion

    /// <summary>
    ///     Determines whether the given pin is compatible with this node data's node.
    /// </summary>
    /// <param name="pin">
    ///     The pin to check compatibility with, if <see langword="null" /> then the node data is always
    ///     considered compatible.
    /// </param>
    /// <returns>
    ///     <see langword="true" /> if the pin is compatible with this node data's node; otherwise <see langword="false" />.
    /// </returns>
    public bool IsCompatibleWithPin(IPin? pin)
    {
        if (pin == null)
            return true;

        if (pin.Direction == PinDirection.Input)
            return OutputType != null && pin.IsTypeCompatible(OutputType);
        return InputType != null && pin.IsTypeCompatible(InputType);
    }

    /// <summary>
    ///     Determines whether the given text matches this node data for a search query.
    /// </summary>
    /// <param name="text">The text to search for.</param>
    /// <returns>
    ///     <see langword="true" /> if the node matches; otherwise <see langword="false" />.
    /// </returns>
    public bool MatchesSearch(string text)
    {
        string rawText = text.Trim();
        text = text.Trim().TrimStart('!');

        if (rawText.StartsWith("!!"))
            return Name.Equals(text, StringComparison.InvariantCultureIgnoreCase);
        else if (rawText.StartsWith("!"))
            return Name.StartsWith(text, StringComparison.InvariantCultureIgnoreCase);
        else
            return Name.Contains(text, StringComparison.InvariantCultureIgnoreCase) ||
                   Description.Contains(text, StringComparison.InvariantCultureIgnoreCase) ||
                   Category.Contains(text, StringComparison.InvariantCultureIgnoreCase);
    }

    #region Properties & Fields
    
    /// <summary>
    ///     Gets the node provider that provided this node data
    /// </summary>
    public NodeProvider Provider { get; }

    /// <summary>
    ///     Gets the type of <see cref="INode" /> this data represents
    /// </summary>
    public Type Type { get; }

    /// <summary>
    ///     Gets the name of the node this data represents
    /// </summary>
    public string Name { get; }

    /// <summary>
    ///     Gets the description of the node this data represents
    /// </summary>
    public string Description { get; }

    /// <summary>
    ///     Gets the category of the node this data represents
    /// </summary>
    public string Category { get; }

    /// <summary>
    ///     Gets the help URL of the node this data represents
    /// </summary>
    public string HelpUrl { get; }

    /// <summary>
    ///     Gets the primary input type of the node this data represents
    /// </summary>
    public Type? InputType { get; }

    /// <summary>
    ///     Gets the primary output of the node this data represents
    /// </summary>
    public Type? OutputType { get; }

    #endregion
}
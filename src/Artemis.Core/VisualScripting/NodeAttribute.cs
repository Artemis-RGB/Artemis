using System;

namespace Artemis.Core;

/// <summary>
///     Represents an attribute that can be used to provide metadata on a node
/// </summary>
public class NodeAttribute : Attribute
{
    #region Properties & Fields

    /// <summary>
    ///     Gets the name of the node
    /// </summary>
    public string Name { get; }

    /// <summary>
    ///     Gets the description of the node
    /// </summary>
    public string Description { get; } = string.Empty;

    /// <summary>
    ///     Gets the category of the node
    /// </summary>
    public string Category { get; } = string.Empty;

    /// <summary>
    /// Gets the help URL of the node
    /// </summary>
    public string HelpUrl { get; init; } = string.Empty;

    /// <summary>
    ///     Gets  the primary input type of the node
    /// </summary>
    public Type? InputType { get; init; }

    /// <summary>
    ///     Gets the primary output type of the node
    /// </summary>
    public Type? OutputType { get; init; }

    #endregion

    #region Constructors

    /// <summary>
    ///     Creates a new instance of the <see cref="NodeAttribute" /> class
    /// </summary>
    public NodeAttribute(string name)
    {
        Name = name;
    }

    /// <summary>
    ///     Creates a new instance of the <see cref="NodeAttribute" /> class
    /// </summary>
    public NodeAttribute(string name, string description)
    {
        Name = name;
        Description = description;
    }

    /// <summary>
    ///     Creates a new instance of the <see cref="NodeAttribute" /> class
    /// </summary>
    public NodeAttribute(string name, string description, string category)
    {
        Name = name;
        Description = description;
        Category = category;
    }

    /// <summary>
    ///     Creates a new instance of the <see cref="NodeAttribute" /> class
    /// </summary>
    public NodeAttribute(string name, string description, string category, string helpUrl)
    {
        Name = name;
        Description = description;
        Category = category;
        HelpUrl = helpUrl;
    }

    #endregion
}
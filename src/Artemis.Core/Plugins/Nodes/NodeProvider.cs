using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using SkiaSharp;

namespace Artemis.Core.Nodes;

/// <summary>
///     Allows you to register one or more <see cref="INode" />s usable by node scripts.
/// </summary>
public abstract class NodeProvider : PluginFeature
{
    private readonly List<NodeData> _nodeDescriptors;

    /// <summary>
    /// Creates a new instance of the <see cref="NodeProvider"/> class.
    /// </summary>
    public NodeProvider()
    {
        _nodeDescriptors = new List<NodeData>();
        NodeDescriptors = new ReadOnlyCollection<NodeData>(_nodeDescriptors);
        Disabled += OnDisabled;
    }

    /// <summary>
    ///     A read-only collection of all nodes added with <see cref="RegisterNodeType{T}" />
    /// </summary>
    public ReadOnlyCollection<NodeData> NodeDescriptors { get; set; }

    /// <summary>
    ///     Adds a node descriptor for a given node, so that it appears in the UI.
    ///     <para>Note: You do not need to manually remove these on disable</para>
    /// </summary>
    /// <typeparam name="T">The type of the node you wish to register</typeparam>
    protected void RegisterNodeType<T>() where T : INode
    {
        RegisterNodeType(typeof(T));
    }
    
    /// <summary>
    ///     Adds a node descriptor for a given node, so that it appears in the UI.
    ///     <para>Note: You do not need to manually remove these on disable</para>
    /// </summary>
    /// <param name="nodeType">The type of the node you wish to register</param>
    protected void RegisterNodeType(Type nodeType)
    {
        if (!IsEnabled)
            throw new ArtemisPluginFeatureException(this, "Can only add a node descriptor when the plugin is enabled");
        if (nodeType == null)
            throw new ArgumentNullException(nameof(nodeType));
        if (!nodeType.IsAssignableTo(typeof(INode))) 
            throw new ArgumentException("Node has to be a base type of the Node-Type.", nameof(nodeType));

        NodeAttribute? nodeAttribute = nodeType.GetCustomAttribute<NodeAttribute>();
        string name = nodeAttribute?.Name ?? nodeType.Name;
        string description = nodeAttribute?.Description ?? string.Empty;
        string category = nodeAttribute?.Category ?? string.Empty;
        string helpUrl = nodeAttribute?.HelpUrl ?? string.Empty;
        
        NodeData nodeData = new(this, nodeType, name, description, category, helpUrl, nodeAttribute?.InputType, nodeAttribute?.OutputType);
        _nodeDescriptors.Add(nodeData);
        NodeTypeStore.Add(nodeData);
    }
    
    protected TypeColorRegistration RegisterTypeColor<T>(SKColor color)
    {
        return NodeTypeStore.AddColor(typeof(T), color, this);
    }
    
    private void OnDisabled(object? sender, EventArgs e)
    {
        // The store will clean up the registrations by itself, the plugin feature just needs to clear its own list
        _nodeDescriptors.Clear();
    }
}
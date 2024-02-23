using System;
using Artemis.Storage.Entities.Profile.Nodes;
using SkiaSharp;

namespace Artemis.Core;

/// <summary>
///     Represents a registration for a type of <see cref="INode" />
/// </summary>
public class NodeTypeRegistration
{
    internal NodeTypeRegistration(NodeData nodeData, PluginFeature pluginFeature)
    {
        NodeData = nodeData;
        PluginFeature = pluginFeature;

        PluginFeature.Disabled += OnDisabled;
    }

    /// <summary>
    ///     Gets the node data that was registered
    /// </summary>
    public NodeData NodeData { get; }

    /// <summary>
    ///     Gets the plugin feature the node is associated with
    /// </summary>
    public PluginFeature PluginFeature { get; }

    /// <summary>
    ///     Gets a boolean indicating whether the registration is in the internal Core store
    /// </summary>
    public bool IsInStore { get; internal set; }

    /// <summary>
    ///     Determines whether the provided entity matches this node type registration.
    /// </summary>
    /// <param name="entity">The entity to check.</param>
    /// <returns><see langword="true" /> if the entity matches this registration; otherwise <see langword="false" />.</returns>
    public bool MatchesEntity(NodeEntity entity)
    {
        return PluginFeature.Id == entity.ProviderId && NodeData.Type.Name == entity.Type;
    }

    private void OnDisabled(object? sender, EventArgs e)
    {
        PluginFeature.Disabled -= OnDisabled;
        if (IsInStore)
            NodeTypeStore.Remove(this);
    }
}

/// <summary>
///     Represents a registration for a <see cref="SKColor" /> to associate with a certain <see cref="System.Type" />
/// </summary>
public class TypeColorRegistration
{
    internal TypeColorRegistration(Type type, SKColor color, PluginFeature pluginFeature)
    {
        Type = type;
        Color = color;
        PluginFeature = pluginFeature;

        PluginFeature.Disabled += OnDisabled;
    }

    /// <summary>
    ///     Gets the type
    /// </summary>
    public Type Type { get; }

    /// <summary>
    ///     Gets the color
    /// </summary>
    public SKColor Color { get; }

    /// <summary>
    ///     Gets a darkened tone of the <see cref="Color" />
    /// </summary>
    public SKColor DarkenedColor => Color.Darken(0.35f);

    /// <summary>
    ///     Gets the plugin feature this type color is associated with
    /// </summary>
    public PluginFeature PluginFeature { get; }

    /// <summary>
    ///     Gets a boolean indicating whether the registration is in the internal Core store
    /// </summary>
    public bool IsInStore { get; internal set; }

    private void OnDisabled(object? sender, EventArgs e)
    {
        PluginFeature.Disabled -= OnDisabled;
        if (IsInStore)
            NodeTypeStore.RemoveColor(this);
    }
}
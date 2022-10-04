using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Artemis.Storage.Entities.Profile.Nodes;
using Newtonsoft.Json;
using Ninject;
using SkiaSharp;

namespace Artemis.Core.Services;

internal class NodeService : INodeService
{
    #region Constants

    private static readonly Type TYPE_NODE = typeof(INode);

    #endregion

    private readonly IKernel _kernel;

    #region Constructors

    public NodeService(IKernel kernel)
    {
        _kernel = kernel;
    }

    #endregion

    #region Properties & Fields

    public IEnumerable<NodeData> AvailableNodes => NodeTypeStore.GetAll();

    #endregion

    #region Methods

    /// <inheritdoc />
    public List<Type> GetRegisteredTypes()
    {
        return NodeTypeStore.GetColors().Select(c => c.Type).Distinct().ToList();
    }

    /// <inheritdoc />
    public TypeColorRegistration GetTypeColorRegistration(Type type)
    {
        TypeColorRegistration? match = NodeTypeStore.GetColor(type);
        if (match != null)
            return match;

        // Objects represent an input that can take any type, these are hardcoded white
        if (type == typeof(object))
            return new TypeColorRegistration(type, new SKColor(255, 255, 255, 255), Constants.CorePlugin);

        // Come up with a random color based on the type name that should be the same each time
        MD5 md5Hasher = MD5.Create();
        byte[] hashed = md5Hasher.ComputeHash(Encoding.UTF8.GetBytes(type.FullName!));
        int hash = BitConverter.ToInt32(hashed, 0);

        SKColor baseColor = SKColor.FromHsl(hash % 255, 50 + hash % 50, 50);
        return new TypeColorRegistration(type, baseColor, Constants.CorePlugin);
    }

    public NodeTypeRegistration RegisterNodeType(Plugin plugin, Type nodeType)
    {
        if (plugin == null) throw new ArgumentNullException(nameof(plugin));
        if (nodeType == null) throw new ArgumentNullException(nameof(nodeType));

        if (!TYPE_NODE.IsAssignableFrom(nodeType)) throw new ArgumentException("Node has to be a base type of the Node-Type.", nameof(nodeType));

        NodeAttribute? nodeAttribute = nodeType.GetCustomAttribute<NodeAttribute>();
        string name = nodeAttribute?.Name ?? nodeType.Name;
        string description = nodeAttribute?.Description ?? string.Empty;
        string category = nodeAttribute?.Category ?? string.Empty;
        string helpUrl = nodeAttribute?.HelpUrl ?? string.Empty;
        
        NodeData nodeData = new(plugin, nodeType, name, description, category, helpUrl, nodeAttribute?.InputType, nodeAttribute?.OutputType, (s, e) => CreateNode(s, e, nodeType));
        return NodeTypeStore.Add(nodeData);
    }

    public TypeColorRegistration RegisterTypeColor(Plugin plugin, Type type, SKColor color)
    {
        if (plugin == null) throw new ArgumentNullException(nameof(plugin));
        if (type == null) throw new ArgumentNullException(nameof(type));

        return NodeTypeStore.AddColor(type, color, plugin);
    }

    public string ExportScript(NodeScript nodeScript)
    {
        nodeScript.Save();
        return JsonConvert.SerializeObject(nodeScript.Entity, IProfileService.ExportSettings);
    }

    public void ImportScript(string json, NodeScript target)
    {
        NodeScriptEntity? entity = JsonConvert.DeserializeObject<NodeScriptEntity>(json);
        if (entity == null)
            throw new ArtemisCoreException("Failed to load node script");

        target.LoadFromEntity(entity);
    }

    private INode CreateNode(INodeScript script, NodeEntity? entity, Type nodeType)
    {
        INode node = _kernel.Get(nodeType) as INode ?? throw new InvalidOperationException($"Node {nodeType} is not an INode");

        if (entity != null)
        {
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
}

/// <summary>
///     A service that provides access to the node system
/// </summary>
public interface INodeService : IArtemisService
{
    /// <summary>
    ///     Gets all available nodes
    /// </summary>
    IEnumerable<NodeData> AvailableNodes { get; }

    /// <summary>
    /// Gets all currently available node pin types.
    /// </summary>
    /// <returns>A <see cref="List{T}"/> of <see cref="Type"/> containing the currently available node pin types.</returns>
    List<Type> GetRegisteredTypes();

    /// <summary>
    ///     Gets the best matching registration for the provided type
    /// </summary>
    TypeColorRegistration GetTypeColorRegistration(Type type);

    /// <summary>
    ///     Registers a node of the provided <paramref name="nodeType" />
    /// </summary>
    /// <param name="plugin">The plugin the node belongs to</param>
    /// <param name="nodeType">The type of node to initialize</param>
    NodeTypeRegistration RegisterNodeType(Plugin plugin, Type nodeType);

    /// <summary>
    ///     Registers a type with a provided color for use in the node editor
    /// </summary>
    /// <param name="plugin">The plugin making the registration</param>
    /// <param name="type">The type to associate the color with</param>
    /// <param name="color">The color to display</param>
    TypeColorRegistration RegisterTypeColor(Plugin plugin, Type type, SKColor color);

    /// <summary>
    ///     Exports the provided node script to JSON.
    /// </summary>
    /// <param name="nodeScript">The node script to export.</param>
    /// <returns>The resulting JSON.</returns>
    string ExportScript(NodeScript nodeScript);

    /// <summary>
    ///     Imports the provided JSON onto the provided node script, overwriting any existing contents.
    /// </summary>
    /// <param name="json">The JSON to import.</param>
    /// <param name="target">The target node script whose contents to overwrite.</param>
    void ImportScript(string json, NodeScript target);
}
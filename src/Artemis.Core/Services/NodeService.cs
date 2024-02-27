using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Artemis.Storage.Entities.Profile.Nodes;
using SkiaSharp;

namespace Artemis.Core.Services;

internal class NodeService : INodeService
{
    public IEnumerable<NodeData> AvailableNodes => NodeTypeStore.GetAll();

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
            return new TypeColorRegistration(type, new SKColor(255, 255, 255, 255), Constants.CorePluginFeature);

        // Come up with a random color based on the type name that should be the same each time
        MD5 md5Hasher = MD5.Create();
        byte[] hashed = md5Hasher.ComputeHash(Encoding.UTF8.GetBytes(type.FullName!));
        int hash = BitConverter.ToInt32(hashed, 0);

        SKColor baseColor = SKColor.FromHsl(hash % 255, 50 + hash % 50, 50);
        return new TypeColorRegistration(type, baseColor, Constants.CorePluginFeature);
    }

    public string ExportScript(NodeScript nodeScript)
    {
        nodeScript.Save();
        return CoreJson.Serialize(nodeScript.Entity);
    }

    public void ImportScript(string json, NodeScript target)
    {
        NodeScriptEntity? entity = CoreJson.Deserialize<NodeScriptEntity>(json);
        if (entity == null)
            throw new ArtemisCoreException("Failed to load node script");

        target.LoadFromEntity(entity);
    }
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
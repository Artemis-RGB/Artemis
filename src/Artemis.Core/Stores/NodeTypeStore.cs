using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.Storage.Entities.Profile.Nodes;
using SkiaSharp;

namespace Artemis.Core;

internal class NodeTypeStore
{
    private static readonly List<NodeTypeRegistration> Registrations = [];
    private static readonly List<TypeColorRegistration> ColorRegistrations = [];

    public static NodeTypeRegistration Add(NodeData nodeData)
    {
        NodeTypeRegistration typeRegistration;
        lock (Registrations)
        {
            if (Registrations.Any(r => r.NodeData == nodeData))
                throw new ArtemisCoreException($"Data binding modifier type store already contains modifier '{nodeData.Name}'");

            typeRegistration = new NodeTypeRegistration(nodeData, nodeData.Provider) {IsInStore = true};
            Registrations.Add(typeRegistration);
        }

        OnNodeTypeAdded(new NodeTypeStoreEvent(typeRegistration));
        return typeRegistration;
    }

    public static void Remove(NodeTypeRegistration typeRegistration)
    {
        lock (Registrations)
        {
            if (!Registrations.Contains(typeRegistration))
                throw new ArtemisCoreException($"Node type store does not contain modifier type '{typeRegistration.NodeData.Name}'");

            Registrations.Remove(typeRegistration);
            typeRegistration.IsInStore = false;
        }

        OnNodeTypeRemoved(new NodeTypeStoreEvent(typeRegistration));
    }

    public static IEnumerable<NodeData> GetAll()
    {
        lock (Registrations)
        {
            return Registrations.Select(r => r.NodeData).ToList();
        }
    }

    public static NodeTypeRegistration? Get(NodeEntity entity)
    {
        lock (Registrations)
        {
            return Registrations.FirstOrDefault(r => r.MatchesEntity(entity));
        }
    }

    public static TypeColorRegistration AddColor(Type type, SKColor color, PluginFeature pluginFeature)
    {
        TypeColorRegistration typeColorRegistration;
        lock (ColorRegistrations)
        {
            typeColorRegistration = new TypeColorRegistration(type, color, pluginFeature) {IsInStore = true};
            ColorRegistrations.Add(typeColorRegistration);
        }

        return typeColorRegistration;
    }

    public static void RemoveColor(TypeColorRegistration typeColorRegistration)
    {
        lock (ColorRegistrations)
        {
            if (!ColorRegistrations.Contains(typeColorRegistration))
                throw new ArtemisCoreException($"Node color store does not contain modifier type '{typeColorRegistration.Type.Name}'");

            ColorRegistrations.Remove(typeColorRegistration);
            typeColorRegistration.IsInStore = false;
        }
    }

    public static List<TypeColorRegistration> GetColors()
    {
        lock (ColorRegistrations)
        {
            return [..ColorRegistrations];
        }
    }

    public static TypeColorRegistration? GetColor(Type type)
    {
        lock (ColorRegistrations)
        {
            return ColorRegistrations
                .OrderByDescending(r => r.Type.ScoreCastability(type))
                .FirstOrDefault(r => r.Type.ScoreCastability(type) > 0);
        }
    }

    public static event EventHandler<NodeTypeStoreEvent>? NodeTypeAdded;
    public static event EventHandler<NodeTypeStoreEvent>? NodeTypeRemoved;

    private static void OnNodeTypeAdded(NodeTypeStoreEvent e)
    {
        NodeTypeAdded?.Invoke(null, e);
    }

    private static void OnNodeTypeRemoved(NodeTypeStoreEvent e)
    {
        NodeTypeRemoved?.Invoke(null, e);
    }
}
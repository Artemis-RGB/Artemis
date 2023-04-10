using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.Storage.Entities.Profile;
using Artemis.Storage.Entities.Profile.Nodes;
using Artemis.Storage.Migrations.Interfaces;
using LiteDB;

namespace Artemis.Storage.Migrations;

public class M0021GradientNodes : IStorageMigration
{
    private void MigrateDataBinding(PropertyEntity property)
    {
        NodeScriptEntity script = property.DataBinding.NodeScript;
        NodeEntity exitNode = script.Nodes.FirstOrDefault(s => s.IsExitNode);
        if (exitNode == null)
            return;

        // Create a new node at the same position of the exit node
        NodeEntity gradientNode = new()
        {
            Id = Guid.NewGuid(),
            Type = "ColorGradientNode",
            PluginId = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff"),
            Name = "Color Gradient",
            Description = "Outputs a color gradient with the given colors",
            X = exitNode.X,
            Y = exitNode.Y,
            Storage = property.Value // Copy the value of the property into the node storage
        };
        script.Nodes.Add(gradientNode);

        // Move all connections of the exit node to the new node
        foreach (NodeConnectionEntity connection in script.Connections)
        {
            if (connection.SourceNode == exitNode.Id)
            {
                connection.SourceNode = gradientNode.Id;
                connection.SourcePinId++;
            }
        }

        // Connect the data binding node to the source node
        script.Connections.Add(new NodeConnectionEntity
        {
            SourceType = "ColorGradient",
            SourceNode = exitNode.Id,
            SourcePinCollectionId = -1,
            SourcePinId = 0,
            TargetType = "ColorGradient",
            TargetNode = gradientNode.Id,
            TargetPinCollectionId = -1,
            TargetPinId = 0
        });

        // Move the exit node to the right
        exitNode.X += 300;
        exitNode.Y += 30;
    }

    private void MigrateDataBinding(PropertyGroupEntity propertyGroup)
    {
        foreach (PropertyGroupEntity propertyGroupPropertyGroup in propertyGroup.PropertyGroups)
            MigrateDataBinding(propertyGroupPropertyGroup);

        foreach (PropertyEntity property in propertyGroup.Properties)
        {
            if (property.Value.StartsWith("[{\"Color\":\"") && property.DataBinding?.NodeScript != null && property.DataBinding.IsEnabled)
                MigrateDataBinding(property);
        }
    }

    public int UserVersion => 21;

    public void Apply(LiteRepository repository)
    {
        // Find all color gradient data bindings, there's no really good way to do this so infer it from the value
        List<ProfileEntity> profiles = repository.Query<ProfileEntity>().ToList();
        foreach (ProfileEntity profileEntity in profiles)
        {
            foreach (LayerEntity layer in profileEntity.Layers.Where(le => le.LayerBrush != null))
                MigrateDataBinding(layer.LayerBrush.PropertyGroup);

            repository.Update(profileEntity);
        }
    }
}
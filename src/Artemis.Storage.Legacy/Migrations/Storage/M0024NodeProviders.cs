using LiteDB;

namespace Artemis.Storage.Legacy.Migrations.Storage;

internal class M0024NodeProviders : IStorageMigration
{
    public int UserVersion => 24;

    public void Apply(LiteRepository repository)
    {
        ILiteCollection<BsonDocument> categoryCollection = repository.Database.GetCollection("ProfileCategoryEntity");
        List<BsonDocument> categoriesToUpdate = new();
        foreach (BsonDocument profileCategoryBson in categoryCollection.FindAll())
        {
            BsonArray? profiles = profileCategoryBson["ProfileConfigurations"]?.AsArray;
            if (profiles != null)
            {
                foreach (BsonValue profile in profiles)
                    profile["Version"] = 1;
                categoriesToUpdate.Add(profileCategoryBson);
            }
        }

        categoryCollection.Update(categoriesToUpdate);

        ILiteCollection<BsonDocument> collection = repository.Database.GetCollection("ProfileEntity");
        List<BsonDocument> profilesToUpdate = new();
        foreach (BsonDocument profileBson in collection.FindAll())
        {
            BsonArray? folders = profileBson["Folders"]?.AsArray;
            BsonArray? layers = profileBson["Layers"]?.AsArray;

            if (folders != null)
                foreach (BsonValue folder in folders)
                    MigrateProfileElement(folder.AsDocument);

            if (layers != null)
                foreach (BsonValue layer in layers)
                {
                    MigrateProfileElement(layer.AsDocument);
                    MigratePropertyGroup(layer.AsDocument["GeneralPropertyGroup"].AsDocument);
                    MigratePropertyGroup(layer.AsDocument["TransformPropertyGroup"].AsDocument);
                    MigratePropertyGroup(layer.AsDocument["LayerBrush"]?["PropertyGroup"].AsDocument);
                }

            profilesToUpdate.Add(profileBson);
        }

        collection.Update(profilesToUpdate);
    }

    private void MigrateProfileElement(BsonDocument profileElement)
    {
        BsonArray? layerEffects = profileElement["LayerEffects"]?.AsArray;
        if (layerEffects != null)
            foreach (BsonValue layerEffect in layerEffects)
                MigratePropertyGroup(layerEffect.AsDocument["PropertyGroup"].AsDocument);

        BsonValue? displayCondition = profileElement["DisplayCondition"];
        if (displayCondition != null)
            MigrateNodeScript(displayCondition.AsDocument["Script"].AsDocument);
    }

    private void MigratePropertyGroup(BsonDocument? propertyGroup)
    {
        if (propertyGroup == null || propertyGroup.Keys.Count == 0)
            return;

        BsonArray? properties = propertyGroup["Properties"]?.AsArray;
        BsonArray? propertyGroups = propertyGroup["PropertyGroups"]?.AsArray;

        if (properties != null)
            foreach (BsonValue property in properties)
                MigrateNodeScript(property.AsDocument["DataBinding"]?["NodeScript"]?.AsDocument);

        if (propertyGroups != null)
            foreach (BsonValue childPropertyGroup in propertyGroups)
                MigratePropertyGroup(childPropertyGroup.AsDocument);
    }

    private void MigrateNodeScript(BsonDocument? nodeScript)
    {
        if (nodeScript == null || nodeScript.Keys.Count == 0)
            return;

        BsonArray? nodes = nodeScript["Nodes"]?.AsArray;
        if (nodes == null)
            return;

        foreach (BsonValue node in nodes)
        {
            node.AsDocument["Type"] = node.AsDocument["Type"]?.AsString?.Replace("Artemis.VisualScripting.Nodes", "Artemis.Plugins.Nodes.General.Nodes");
            node.AsDocument["ProviderId"] = "Artemis.Plugins.Nodes.General.GeneralNodesProvider-d9e1ee78";
        }
    }
}
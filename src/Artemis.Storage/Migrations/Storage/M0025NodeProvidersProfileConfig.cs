using LiteDB;

namespace Artemis.Storage.Migrations.Storage;

public class M0025NodeProvidersProfileConfig : IStorageMigration
{
    public int UserVersion => 25;

    public void Apply(LiteRepository repository)
    {
        ILiteCollection<BsonDocument> categoryCollection = repository.Database.GetCollection("ProfileCategoryEntity");
        foreach (BsonDocument profileCategoryBson in categoryCollection.FindAll())
        {
            BsonArray? profiles = profileCategoryBson["ProfileConfigurations"]?.AsArray;
            if (profiles != null)
            {
                foreach (BsonValue profile in profiles)
                {
                    profile["Version"] = 2;
                    MigrateNodeScript(profile["ActivationCondition"]?.AsDocument);
                }
            }
            categoryCollection.Update(profileCategoryBson);
        }
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
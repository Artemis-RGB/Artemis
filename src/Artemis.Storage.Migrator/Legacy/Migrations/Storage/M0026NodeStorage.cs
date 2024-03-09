using System.Text.Json;
using System.Text.Json.Nodes;
using LiteDB;
using Serilog;

namespace Artemis.Storage.Migrator.Legacy.Migrations.Storage;

public class M0026NodeStorage : IStorageMigration
{
    private readonly ILogger _logger;

    public M0026NodeStorage(ILogger logger)
    {
        _logger = logger;
    }

    public int UserVersion => 26;

    public void Apply(LiteRepository repository)
    {
        ILiteCollection<BsonDocument> categoryCollection = repository.Database.GetCollection("ProfileCategoryEntity");
        List<BsonDocument> toUpdate = new();
        foreach (BsonDocument profileCategoryBson in categoryCollection.FindAll())
        {
            BsonArray? profiles = profileCategoryBson["ProfileConfigurations"]?.AsArray;
            if (profiles != null)
            {
                foreach (BsonValue profile in profiles)
                {
                    profile["Version"] = 4;
                    MigrateNodeScript(profile["ActivationCondition"]?.AsDocument);
                }

                toUpdate.Add(profileCategoryBson);
            }
        }

        categoryCollection.Update(toUpdate);

        ILiteCollection<BsonDocument> collection = repository.Database.GetCollection("ProfileEntity");
        List<BsonDocument> profilesToUpdate = new();
        foreach (BsonDocument profileBson in collection.FindAll())
        {
            BsonArray? folders = profileBson["Folders"]?.AsArray;
            BsonArray? layers = profileBson["Layers"]?.AsArray;

            if (folders != null)
            {
                foreach (BsonValue folder in folders)
                    MigrateProfileElement(folder.AsDocument);
            }

            if (layers != null)
            {
                foreach (BsonValue layer in layers)
                {
                    MigrateProfileElement(layer.AsDocument);
                    MigratePropertyGroup(layer.AsDocument["GeneralPropertyGroup"].AsDocument);
                    MigratePropertyGroup(layer.AsDocument["TransformPropertyGroup"].AsDocument);
                    MigratePropertyGroup(layer.AsDocument["LayerBrush"]?["PropertyGroup"].AsDocument);
                }
            }

            profilesToUpdate.Add(profileBson);
        }

        collection.Update(profilesToUpdate);
    }

    private void MigrateProfileElement(BsonDocument profileElement)
    {
        BsonArray? layerEffects = profileElement["LayerEffects"]?.AsArray;
        if (layerEffects != null)
        {
            foreach (BsonValue layerEffect in layerEffects)
                MigratePropertyGroup(layerEffect.AsDocument["PropertyGroup"].AsDocument);
        }

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
        {
            foreach (BsonValue property in properties)
                MigrateNodeScript(property.AsDocument["DataBinding"]?["NodeScript"]?.AsDocument);
        }

        if (propertyGroups != null)
        {
            foreach (BsonValue childPropertyGroup in propertyGroups)
                MigratePropertyGroup(childPropertyGroup.AsDocument);
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
            // Migrate the storage of the node 
            node["Storage"] = MigrateNodeStorageJson(node.AsDocument["Storage"]?.AsString, _logger);
        }
    }

    private static string? MigrateNodeStorageJson(string? json, ILogger logger)
    {
        if (string.IsNullOrEmpty(json))
            return json;

        try
        {
            JsonDocument jsonDocument = JsonDocument.Parse(json);
            if (jsonDocument.RootElement.ValueKind != JsonValueKind.Object)
                return json;

            JsonObject? jsonObject = JsonObject.Create(jsonDocument.RootElement);
            if (jsonObject == null)
                return json;

            if (jsonObject["$type"] != null && jsonObject["$values"] != null)
            {
                JsonArray? values = jsonObject["$values"]?.AsArray();
                if (values != null)
                {
                    foreach (JsonNode? jsonNode in values.ToList())
                    {
                        if (jsonNode is JsonObject childObject)
                            ConvertToSystemTextJson(childObject);
                    }

                    return values.ToJsonString();
                }
            }
            else
            {
                ConvertToSystemTextJson(jsonObject);
            }

            return jsonObject.ToJsonString();
        }
        catch (Exception e)
        {
            logger.Error(e, "Failed to migrate node storage JSON");
            return json;
        }
    }

    private static void ConvertToSystemTextJson(JsonObject jsonObject)
    {
        FilterType(jsonObject);

        // Recursively convert all JSON arrays from {$type: "xyz", $values: []} to []
        foreach ((string? key, JsonNode? value) in jsonObject.ToDictionary())
        {
            if (value is not JsonObject obj)
                continue;

            // if there is a $type and a $values, replace the entire node with $values regardless of the value of $type
            if (obj["$type"] != null && obj["$values"] != null)
            {
                JsonArray? values = obj["$values"]?.AsArray();
                if (values != null)
                {
                    obj.Remove("$values");
                    jsonObject[key] = values;
                    foreach (JsonNode? jsonNode in values.ToList())
                    {
                        if (jsonNode is JsonObject childObject)
                            ConvertToSystemTextJson(childObject);
                    }
                }

                obj.Remove("$type");
            }
            else
            {
                ConvertToSystemTextJson(obj);
            }
        }
    }

    private static void FilterType(JsonObject jsonObject)
    {
        // Replace or remove $type depending on whether there's a matching JsonDerivedType
        // This could be done with reflection but that would mean this migration automatically gains new behaviour over time.
        JsonNode? type = jsonObject["$type"];
        if (type != null)
        {
            // Adaption Hints
            if (type.GetValue<string>() == "Artemis.Storage.Entities.Profile.AdaptionHints.CategoryAdaptionHintEntity, Artemis.Storage")
                jsonObject["$type"] = "Category";
            else if (type.GetValue<string>() == "Artemis.Storage.Entities.Profile.AdaptionHints.DeviceAdaptionHintEntity, Artemis.Storage")
                jsonObject["$type"] = "Device";
            else if (type.GetValue<string>() == "Artemis.Storage.Entities.Profile.AdaptionHints.KeyboardSectionAdaptionHintEntity, Artemis.Storage")
                jsonObject["$type"] = "KeyboardSection";
            else if (type.GetValue<string>() == "Artemis.Storage.Entities.Profile.AdaptionHints.SingleLedAdaptionHintEntity, Artemis.Storage")
                jsonObject["$type"] = "SingleLed";
            // Conditions
            else if (type.GetValue<string>() == "Artemis.Storage.Entities.Profile.Conditions.AlwaysOnConditionEntity, Artemis.Storage")
                jsonObject["$type"] = "AlwaysOn";
            else if (type.GetValue<string>() == "Artemis.Storage.Entities.Profile.Conditions.EventConditionEntity, Artemis.Storage")
                jsonObject["$type"] = "Event";
            else if (type.GetValue<string>() == "Artemis.Storage.Entities.Profile.Conditions.PlayOnceConditionEntity, Artemis.Storage")
                jsonObject["$type"] = "PlayOnce";
            else if (type.GetValue<string>() == "Artemis.Storage.Entities.Profile.Conditions.StaticConditionEntity, Artemis.Storage")
                jsonObject["$type"] = "Static";
            else
                jsonObject.Remove("$type");
        }
    }
}
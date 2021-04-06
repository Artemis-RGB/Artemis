using System.Collections.Generic;
using Artemis.Storage.Entities.Module;
using Artemis.Storage.Migrations.Interfaces;
using LiteDB;

namespace Artemis.Storage.Migrations
{
    public class M0008PluginFeatures : IStorageMigration
    {
        private void Migrate(BsonValue bsonValue, Dictionary<string, string> pluginMap)
        {
            if (bsonValue.IsArray)
            {
                foreach (BsonValue child in bsonValue.AsArray)
                    Migrate(child, pluginMap);
                return;
            }

            if (bsonValue.IsDocument)
            {
                // Data model paths
                ReplaceIfFound(bsonValue, "DataModelGuid", "DataModelId", pluginMap);
                // Layer effects
                if (bsonValue.AsDocument.ContainsKey("EffectType"))
                    ReplaceIfFound(bsonValue, "PluginGuid", "ProviderId", pluginMap);
                // Properties
                if (bsonValue.AsDocument.ContainsKey("KeyframesEnabled"))
                    ReplaceIfFound(bsonValue, "PluginGuid", "FeatureId", pluginMap);

                foreach (BsonValue documentValue in bsonValue.AsDocument.Values)
                    Migrate(documentValue, pluginMap);
            }
        }

        private bool ReplaceIfFound(BsonValue bsonValue, string oldKey, string newKey, Dictionary<string, string> pluginMap)
        {
            if (bsonValue.AsDocument.TryGetValue(oldKey, out BsonValue dataModelValue))
                if (pluginMap.TryGetValue(dataModelValue.AsGuid.ToString(), out string featureId))
                {
                    bsonValue.AsDocument[newKey] = featureId;
                    bsonValue.AsDocument.Remove(oldKey);

                    return true;
                }

            return false;
        }

        public int UserVersion => 8;

        public void Apply(LiteRepository repository)
        {
            Dictionary<string, string> pluginMap = new()
            {
                {"ffffffff-ffff-ffff-ffff-ffffffffffff", "Artemis.Core.CorePluginFeature-ffffffff"},
                {"ab41d601-35e0-4a73-bf0b-94509b006ab0", "Artemis.Plugins.DataModelExpansions.TestData.PluginDataModelExpansion-ab41d601"},
                {"c20e876f-7cb0-4fa1-b0cc-ae1afb5865d1", "Artemis.Plugins.Devices.Asus.AsusDeviceProvider-c20e876f"},
                {"b78f644b-827f-4bb4-bf03-2adaa365b58b", "Artemis.Plugins.Devices.CoolerMaster.CoolerMasterDeviceProvider-b78f644b"},
                {"926629ab-8170-42f3-be18-22c694aa91cd", "Artemis.Plugins.Devices.Corsair.CorsairDeviceProvider-926629ab"},
                {"cad475d3-c621-4ec7-bbfc-784e3b4723ce", "Artemis.Plugins.Devices.Debug.DebugDeviceProvider-cad475d3"},
                {"6f073d4d-d97d-4040-9750-841fdbe06915", "Artemis.Plugins.Devices.DMX.DMXDeviceProvider-6f073d4d"},
                {"62a45c0c-884c-4868-9fd7-3c5987fe07ca", "Artemis.Plugins.Devices.Logitech.LogitechDeviceProvider-62a45c0c"},
                {"9177c320-1206-48a3-af52-b1749c758786", "Artemis.Plugins.Devices.Msi.MsiDeviceProvider-9177c320"},
                {"a487332f-c4b3-43e7-b80f-f33adc6fff87", "Artemis.Plugins.Devices.Novation.NovationDeviceProvider-a487332f"},
                {"58a3d80e-d5cb-4a40-9465-c0a5d54825d6", "Artemis.Plugins.Devices.Razer.RazerDeviceProvider-58a3d80e"},
                {"10049953-94c1-4102-988b-9e4f0b64c232", "Artemis.Plugins.Devices.Roccat.RoccatDeviceProvider-10049953"},
                {"27945704-6edd-48b4-bc0e-319cce9693fc", "Artemis.Plugins.Devices.SteelSeries.SteelSeriesDeviceProvider-27945704"},
                {"e70fd5ba-9881-480a-8ff6-078ed5f747fa", "Artemis.Plugins.Devices.Wooting.WootingDeviceProvider-e70fd5ba"},
                {"ec86de32-1010-4bf7-97d7-1dcc46659ab6", "Artemis.Plugins.Devices.WS281X.WS281XDeviceProvider-ec86de32"},
                {"92a9d6ba-6f7a-4937-94d5-c1d715b4141a", "Artemis.Plugins.LayerBrushes.Color.ColorBrushProvider-92a9d6ba"},
                {"0bbf931b-87ad-4809-9cd9-bda33f4d4695", "Artemis.Plugins.LayerBrushes.ColorRgbNet.RgbNetColorBrushProvider-0bbf931b"},
                {"61cbbf01-8d69-4ede-a972-f3f269da66d9", "Artemis.Plugins.LayerBrushes.Noise.NoiseBrushProvider-61cbbf01"},
                {"0cb99d89-915b-407e-82ac-8316d0559c4e", "Artemis.Plugins.LayerBrushes.Chroma.ChromaLayerBrushProvider-0cb99d89"},
                {"bf9cf3ac-9f97-4328-b32f-aa39df1698ff", "Artemis.Plugins.LayerBrushes.Gif.GifLayerBrushProvider-bf9cf3ac"},
                {"4570510a-7c8b-4324-b915-cea738a65ac2", "Artemis.Plugins.LayerBrushes.Particle.ParticleLayerBrushProvider-4570510a"},
                {"245aa860-4224-4d1c-ab81-2d6b5593a9fa", "Artemis.Plugins.LayerBrushes.Spectrum.PluginLayerBrushProvider-245aa860"},
                {"fca5b5d6-3f86-4ea7-a271-06ec3fc219e2", "Artemis.Plugins.LayerEffects.Filter.FilterEffectProvider-fca5b5d6"},
                {"0de2991a-d7b8-4f61-ae4e-6623849215b5", "Artemis.Plugins.Modules.General.GeneralModule-0de2991a"},
                {"29e3ff97-83a5-44fc-a2dc-04f446b54146", "Artemis.Plugins.Modules.Overlay.OverlayModule-29e3ff97"},
                {"ea2064cc-63ad-4a22-93e3-bfc71beb6c4b", "Artemis.Plugins.Modules.Fallout4.Fallout4Module-ea2064cc"},
                {"a34dd13c-2d66-47a3-91fc-265749144d01", "Artemis.Plugins.Modules.LeagueOfLegends.LeagueOfLegendsModule-a34dd13c"},
                {"49dff3a6-3d2a-444a-8346-582b2d61b776", "Module.EliteDangerous.EliteDangerousModule-49dff3a6"},
                {"8f493ff1-0590-4c70-8a1d-e599e5580d21", "Artemis.Plugins.Modules.TruckSimulator.TruckSimulatorModule-8f493ff1"},
                {"184ce933-b8ff-465f-b3d2-a23a17b35f65", "Artemis.Plugins.PhilipsHue.HueDataModelExpansion-184ce933"}
            };

            // Remove the default brush the user selected, this will make the UI pick a new one
            repository.Database.Execute("DELETE PluginSettingEntity WHERE $.Name = \"ProfileEditor.DefaultLayerBrushDescriptor\"");

            // Module settings
            repository.Database.GetCollection<ModuleSettingsEntity>().DropIndex("PluginGuid");
            ILiteCollection<BsonDocument> modules = repository.Database.GetCollection("ModuleSettingsEntity");
            foreach (BsonDocument bsonDocument in modules.FindAll())
            {
                if (ReplaceIfFound(bsonDocument, "PluginGuid", "ModuleId", pluginMap))
                    modules.Update(bsonDocument);
                else if (bsonDocument.ContainsKey("PluginGuid"))
                    modules.Delete(bsonDocument["_id"]);
            }
            repository.Database.GetCollection<ModuleSettingsEntity>().EnsureIndex(s => s.ModuleId, true);

            // Profiles
            ILiteCollection<BsonDocument> collection = repository.Database.GetCollection("ProfileEntity");
            foreach (BsonDocument bsonDocument in collection.FindAll())
            {
                ReplaceIfFound(bsonDocument, "PluginGuid", "ModuleId", pluginMap);

                foreach (BsonValue bsonLayer in bsonDocument["Layers"].AsArray)
                {
                    Migrate(bsonLayer, pluginMap);
                    MigrateProperties(bsonLayer, pluginMap);
                }

                foreach (BsonValue bsonLayer in bsonDocument["Folders"].AsArray)
                {
                    Migrate(bsonLayer, pluginMap);
                    MigrateProperties(bsonLayer, pluginMap);
                }

                collection.Update(bsonDocument);
            }
        }

        private void MigrateProperties(BsonValue profileElementBson, Dictionary<string, string> pluginMap)
        {
            foreach (BsonValue bsonValue in profileElementBson["PropertyEntities"].AsArray)
            {
                if (bsonValue["Path"].AsString == "General.BrushReference")
                {
                    bsonValue["Value"] = bsonValue["Value"].AsString.Replace("BrushPluginGuid", "LayerBrushProviderId");
                    foreach ((string key, string value) in pluginMap)
                        bsonValue["Value"] = bsonValue["Value"].AsString.Replace(key, value);
                }
            }
        }
    }
}
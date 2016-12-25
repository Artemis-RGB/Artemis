using System;
using System.Collections.Generic;
using System.IO;
using Artemis.DAL;
using Artemis.Managers;
using Artemis.Models;
using Artemis.Profiles.Layers.Models;
using Artemis.Utilities.DataReaders;
using Newtonsoft.Json;

namespace Artemis.Modules.Games.LightFx
{
    public class LightFxModel : GameModel
    {
        public LightFxModel(DeviceManager deviceManager, LuaManager luaManager, PipeServer pipeServer)
            : base(deviceManager, luaManager, SettingsProvider.Load<LightFxSettings>(), new LightFxDataModel())
        {
            Name = "LightFX";
            ProcessName = "LoL";
            Scale = 4;
            Enabled = Settings.Enabled;
            Initialized = false;

            // This model can enable itself by changing its process name to the currently running Light FX game.
            pipeServer.PipeMessage += PipeServerOnPipeMessage;
        }

        public int Scale { get; set; }

        private void PipeServerOnPipeMessage(string msg)
        {
            // Ensure it's Light FX JSON
            if (!msg.Contains("lightFxState"))
                return;

            // Deserialize and data
            try
            {
                JsonConvert.PopulateObject(msg, DataModel);
            }
            catch (Exception ex)
            {
                Logger?.Error(ex, "Failed to deserialize LightFX JSON");
                throw;
            }

            // Setup process name
            ProcessName = Path.GetFileNameWithoutExtension(((LightFxDataModel) DataModel).LightFxState.game);
        }

        public override void Dispose()
        {
            Initialized = false;
            base.Dispose();
        }

        public override void Enable()
        {
            Initialized = true;
        }

        public override void Update()
        {
        }

        public override List<LayerModel> GetRenderLayers(bool keyboardOnly)
        {
            return Profile.GetRenderLayers(DataModel, keyboardOnly);
        }
    }
}
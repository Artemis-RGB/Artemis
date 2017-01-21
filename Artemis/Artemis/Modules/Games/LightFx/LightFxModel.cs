using System;
using System.IO;
using Artemis.DAL;
using Artemis.Managers;
using Artemis.Modules.Abstract;
using Artemis.Utilities.DataReaders;
using Newtonsoft.Json;

namespace Artemis.Modules.Games.LightFx
{
    public class LightFxModel : ModuleModel
    {
        public LightFxModel(DeviceManager deviceManager, LuaManager luaManager, PipeServer pipeServer)
            : base(deviceManager, luaManager)
        {
            Settings = SettingsProvider.Load<LightFxSettings>();
            DataModel = new LightFxDataModel();
            ProcessNames.Add("LoL");

            // This model can enable itself by changing its process name to the currently running Light FX game.
            pipeServer.PipeMessage += PipeServerOnPipeMessage;
        }

        public override string Name => "LightFX";
        public override bool IsOverlay => false;
        public override bool IsBoundToProcess => true;

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
            var processName = Path.GetFileNameWithoutExtension(((LightFxDataModel) DataModel).LightFxState.game);
            if (!ProcessNames.Contains(processName))
                ProcessNames.Add(processName);
        }

        public override void Update()
        {
        }
    }
}
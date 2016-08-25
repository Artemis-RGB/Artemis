using System;
using System.Collections.Generic;
using Artemis.DAL;
using Artemis.Managers;
using Artemis.Models;
using Artemis.Profiles.Layers.Models;
using Newtonsoft.Json;

namespace Artemis.Modules.Games.UnrealTournament
{
    public class UnrealTournamentModel : GameModel
    {
        public UnrealTournamentModel(MainManager mainManager)
            : base(mainManager, SettingsProvider.Load<UnrealTournamentSettings>(), new UnrealTournamentDataModel())
        {
            Name = "UnrealTournament";
            ProcessName = "UE4-Win64-Shipping";
            Scale = 4;
            Enabled = Settings.Enabled;
            Initialized = false;
        }

        public int Scale { get; set; }

        public override void Dispose()
        {
            Initialized = false;
            MainManager.PipeServer.PipeMessage -= PipeServerOnPipeMessage;
        }

        public override void Enable()
        {
            MainManager.PipeServer.PipeMessage += PipeServerOnPipeMessage;
            Initialized = true;
        }

        private void PipeServerOnPipeMessage(string message)
        {
            if (!message.Contains("\"Environment\":"))
                return;

            // Parse the JSON
            try
            {
                DataModel = JsonConvert.DeserializeObject<UnrealTournamentDataModel>(message);
            }
            catch (Exception)
            {
                //ignored
            }
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
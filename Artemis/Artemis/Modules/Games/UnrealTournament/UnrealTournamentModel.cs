using System;
using System.Collections.Generic;
using System.Timers;
using Artemis.DAL;
using Artemis.Managers;
using Artemis.Models;
using Artemis.Profiles.Layers.Models;
using Newtonsoft.Json;

namespace Artemis.Modules.Games.UnrealTournament
{
    public class UnrealTournamentModel : GameModel
    {
        private Timer _killTimer;
        private int _lastScore;

        public UnrealTournamentModel(MainManager mainManager)
            : base(mainManager, SettingsProvider.Load<UnrealTournamentSettings>(), new UnrealTournamentDataModel())
        {
            Name = "UnrealTournament";
            ProcessName = "UE4-Win64-Shipping";
            Scale = 4;
            Enabled = Settings.Enabled;
            Initialized = false;

            _killTimer = new Timer(3500);
            _killTimer.Elapsed += KillTimerOnElapsed;
        }

        public int Scale { get; set; }

        public override void Dispose()
        {
            Initialized = false;

            _killTimer.Stop();
            MainManager.PipeServer.PipeMessage -= PipeServerOnPipeMessage;
        }

        public override void Enable()
        {
            MainManager.PipeServer.PipeMessage += PipeServerOnPipeMessage;
            _killTimer.Start();

            Initialized = true;
        }

        private void PipeServerOnPipeMessage(string message)
        {
            if (!message.Contains("\"Environment\":"))
                return;

            // Parse the JSON
            try
            {
                JsonConvert.PopulateObject(message, DataModel);
            }
            catch (Exception)
            {
                //ignored
            }
        }

        public override void Update()
        {
            var utDataModel = (UnrealTournamentDataModel) DataModel;
            if (utDataModel.Player?.State?.Score == _lastScore)
                return;

            // Reset the timer
            _killTimer.Stop();
            _killTimer.Start();
            if (utDataModel.Player?.State != null)
            {
                // Can't go past MonsterKill in the current version of UT
                if (utDataModel.Player.KillState != KillState.MonsterKill)
                {
                    var recentKills = utDataModel.Player.State.Score - _lastScore;
                    utDataModel.Player.KillState = (KillState) ((int) utDataModel.Player.KillState + recentKills);
                }
                _lastScore = utDataModel.Player.State.Score;
            }
            else
            {
                _lastScore = 0;
            }
        }

        private void KillTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            var dataModel = (UnrealTournamentDataModel) DataModel;
            if (dataModel.Player != null)
                dataModel.Player.KillState = KillState.None;
        }

        public override List<LayerModel> GetRenderLayers(bool keyboardOnly)
        {
            return Profile.GetRenderLayers(DataModel, keyboardOnly);
        }
    }
}
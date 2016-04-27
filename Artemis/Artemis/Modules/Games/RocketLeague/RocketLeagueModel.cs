using System;
using System.Drawing;
using System.Linq;
using Artemis.Managers;
using Artemis.Models;
using Artemis.Settings;
using Artemis.Utilities;
using Artemis.Utilities.Memory;
using Newtonsoft.Json;

namespace Artemis.Modules.Games.RocketLeague
{
    public class RocketLeagueModel : GameModel
    {
        private Memory _memory;
        private GamePointersCollection _pointer;

        public RocketLeagueModel(MainManager mainManager, RocketLeagueSettings settings) : base(mainManager, settings)
        {
            Name = "RocketLeague";
            ProcessName = "RocketLeague";
            Scale = 4;
            Enabled = Settings.Enabled;
            Initialized = false;
        }

        public int Scale { get; set; }

        public override void Dispose()
        {
            Initialized = false;
            _memory = null;
        }

        public override void Enable()
        {
            Initialized = false;

            Updater.GetPointers();
            _pointer = JsonConvert.DeserializeObject<GamePointersCollection>(Offsets.Default.RocketLeague);

            var tempProcess = MemoryHelpers.GetProcessIfRunning(ProcessName);
            _memory = new Memory(tempProcess);
            GameDataModel = new RocketLeagueDataModel();

            Initialized = true;
        }

        public override void Update()
        {
            if (Profile == null || GameDataModel == null || _memory == null)
                return;

            var offsets = _pointer.GameAddresses.First(ga => ga.Description == "Boost").ToString();
            var boostAddress = _memory.GetAddress("\"RocketLeague.exe\"" + offsets);
            var boostFloat = _memory.ReadFloat(boostAddress)*100/3;

            ((RocketLeagueDataModel) GameDataModel).Boost = (int) Math.Ceiling(boostFloat);

            // Take care of any reading errors resulting in an OutOfMemory on draw
            if (((RocketLeagueDataModel) GameDataModel).Boost < 0)
                ((RocketLeagueDataModel) GameDataModel).Boost = 0;
            if (((RocketLeagueDataModel) GameDataModel).Boost > 100)
                ((RocketLeagueDataModel) GameDataModel).Boost = 100;

            foreach (var layerModel in Profile.Layers)
                layerModel.Update<RocketLeagueDataModel>(GameDataModel);
        }

        public override Bitmap GenerateBitmap()
        {
            if (Profile == null || GameDataModel == null)
                return null;

            var keyboardRect = MainManager.KeyboardManager.ActiveKeyboard.KeyboardRectangle(Scale);
            return Profile.GenerateBitmap<RocketLeagueDataModel>(keyboardRect, GameDataModel);
        }
    }
}
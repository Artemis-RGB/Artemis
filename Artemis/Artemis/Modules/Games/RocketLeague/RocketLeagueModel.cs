using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.Managers;
using Artemis.Models;
using Artemis.Models.Profiles;
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

        public RocketLeagueModel(MainManager mainManager, RocketLeagueSettings settings)
            : base(mainManager, settings, new RocketLeagueDataModel())
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
            if (tempProcess == null)
                return;

            _memory = new Memory(tempProcess);

            Initialized = true;
        }

        public override void Update()
        {
            if (Profile == null || DataModel == null || _memory == null)
                return;

            var offsets = _pointer.GameAddresses.First(ga => ga.Description == "Boost").ToString();
            var boostAddress = _memory.GetAddress("\"RocketLeague.exe\"" + offsets);
            var boostFloat = _memory.ReadFloat(boostAddress)*100/3;

            ((RocketLeagueDataModel) DataModel).Boost = (int) Math.Ceiling(boostFloat);

            // Take care of any reading errors resulting in an OutOfMemory on draw
            if (((RocketLeagueDataModel) DataModel).Boost < 0)
                ((RocketLeagueDataModel) DataModel).Boost = 0;
            if (((RocketLeagueDataModel) DataModel).Boost > 100)
                ((RocketLeagueDataModel) DataModel).Boost = 100;
        }

        public override List<LayerModel> GetRenderLayers(bool renderMice, bool renderHeadsets)
        {
            return Profile.GetRenderLayers<RocketLeagueDataModel>(DataModel, renderMice, renderHeadsets);
        }
    }
}
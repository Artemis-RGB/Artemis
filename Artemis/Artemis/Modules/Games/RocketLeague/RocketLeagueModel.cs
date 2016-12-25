using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.DAL;
using Artemis.Managers;
using Artemis.Models;
using Artemis.Profiles.Layers.Models;
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

        public RocketLeagueModel(DeviceManager deviceManager, LuaManager luaManager) : base(deviceManager, luaManager, SettingsProvider.Load<RocketLeagueSettings>(), new RocketLeagueDataModel())
        {
            Name = "RocketLeague";
            ProcessName = "RocketLeague";
            Scale = 4;
            Enabled = Settings.Enabled;
            Initialized = false;

            // Generate a new offset when the game is updated
            //var offset = new GamePointersCollection
            //{
            //    Game = "RocketLeague",
            //    GameVersion = "1.24",
            //    GameAddresses = new List<GamePointer>
            //    {
            //        new GamePointer
            //        {
            //            Description = "Boost",
            //            BasePointer = new IntPtr(0x016BBFB4),
            //            Offsets = new[] { 0xc4, 0x210, 0x320, 0x734, 0x224}
            //        }
            //    }
            //};
            //var res = JsonConvert.SerializeObject(offset, Formatting.Indented);
        }

        public int Scale { get; set; }

        public override void Dispose()
        {
            Initialized = false;
            _memory = null;
            base.Dispose();
        }

        public override void Enable()
        {
            Initialized = false;

            Updater.GetPointers();
            _pointer = SettingsProvider.Load<OffsetSettings>().RocketLeague;

            var tempProcess = MemoryHelpers.GetProcessIfRunning(ProcessName);
            if (tempProcess == null)
                return;

            _memory = new Memory(tempProcess);

            Initialized = true;
        }

        public override void Update()
        {
            if ((Profile == null) || (DataModel == null) || (_memory == null))
                return;

            var offsets = _pointer.GameAddresses.First(ga => ga.Description == "Boost").ToString();
            var boostAddress = _memory.GetAddress("\"RocketLeague.exe\"" + offsets);
            var boostInt = (int) (_memory.ReadFloat(boostAddress)*100);
            if (boostInt > 100)
                boostInt = 100;
            if (boostInt < 0)
                boostInt = 0;

            ((RocketLeagueDataModel) DataModel).Boost = boostInt;
        }

        public override List<LayerModel> GetRenderLayers(bool keyboardOnly)
        {
            return Profile.GetRenderLayers(DataModel, keyboardOnly);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.DAL;
using Artemis.Managers;
using Artemis.Modules.Abstract;
using Artemis.Settings;
using Artemis.Utilities;
using Artemis.Utilities.Memory;
using Newtonsoft.Json;

namespace Artemis.Modules.Games.RocketLeague
{
    public class RocketLeagueModel : ModuleModel
    {
        private Memory _memory;
        private GamePointersCollection _pointer;

        public RocketLeagueModel(DeviceManager deviceManager, LuaManager luaManager) : base(deviceManager, luaManager)
        {
            Settings = SettingsProvider.Load<RocketLeagueSettings>();
            DataModel = new RocketLeagueDataModel();
            ProcessNames.Add("RocketLeague");

            // Generate a new offset when the game is updated
            var offset = new GamePointersCollection
            {
                Game = "RocketLeague",
                GameVersion = "1.38",
                GameAddresses = new List<GamePointer>
                {
                    new GamePointer
                    {
                        Description = "Boost",
                        BasePointer = new IntPtr(0x0193E31C),
                        Offsets = new[] {0x504, 0x738, 0x14, 0x14, 0x240}
                    }
                }
            };
            var res = JsonConvert.SerializeObject(offset, Formatting.Indented);
        }

        public override string Name => "RocketLeague";
        public override bool IsOverlay => false;
        public override bool IsBoundToProcess => true;

        public override void Dispose()
        {
            base.Dispose();

            _memory?.Dispose();
            _memory = null;
        }

        public override void Enable()
        {
            Updater.GetPointers();
            _pointer = SettingsProvider.Load<OffsetSettings>().RocketLeague;

            base.Enable();
        }

        public override void Update()
        {
            if (_memory == null)
            {
                var tempProcess = MemoryHelpers.GetProcessIfRunning(ProcessNames[0]);
                if (tempProcess == null)
                    return;

                _memory = new Memory(tempProcess);
            }

            if (ProfileModel == null || DataModel == null || _memory == null)
                return;

            var offsets = _pointer.GameAddresses.First(ga => ga.Description == "Boost").ToString();
            var boostAddress = _memory.GetAddress("\"RocketLeague.exe\"" + offsets);
            var boostInt = (int) (_memory.ReadFloat(boostAddress) * 100);
            if (boostInt > 100)
                boostInt = 100;
            if (boostInt < 0)
                boostInt = 0;

            ((RocketLeagueDataModel) DataModel).Boost = boostInt;
        }
    }
}

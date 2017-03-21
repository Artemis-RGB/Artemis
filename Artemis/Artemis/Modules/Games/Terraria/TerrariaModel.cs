using System;
using System.Linq;
using Artemis.DAL;
using Artemis.Managers;
using Artemis.Modules.Abstract;
using Artemis.Settings;
using Artemis.Utilities;
using Artemis.Utilities.Memory;

namespace Artemis.Modules.Games.Terraria
{
    public class TerrariaModel : ModuleModel
    {
        private Memory _memory;
        private GamePointersCollection _pointer;

        public TerrariaModel(DeviceManager deviceManager, LuaManager luaManager) : base(deviceManager, luaManager)
        {
            Settings = SettingsProvider.Load<TerrariaSettings>();
            DataModel = new TerrariaDataModel();
            ProcessNames.Add("Terraria");

            // Generate a new offset when the game is updated
            //_pointer = new GamePointersCollection
            //{
            //    Game = "Terraria",
            //    GameVersion = "1.3.4.4",
            //    GameAddresses = new List<GamePointer>
            //    {
            //        new GamePointer
            //        {
            //            Description = "PlayerBase",
            //            BasePointer = new IntPtr(0x0039C078),
            //            Offsets = new[] {0x280, 0x6C0, 0x674, 0x3C}
            //        }
            //    }
            //};
            //var res = JsonConvert.SerializeObject(_pointer, Formatting.Indented);
        }

        public override string Name => "Terraria";
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
            _pointer = SettingsProvider.Load<OffsetSettings>().Terraria;

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

            var offsets = _pointer.GameAddresses.First(ga => ga.Description == "PlayerBase").ToString();
            var baseAddress = _memory.GetAddress("\"Terraria.exe\"" + offsets);
            var basePointer = new IntPtr(_memory.ReadInt32(baseAddress));
            var playerPointer = new IntPtr(_memory.ReadInt32(basePointer + 0x18));

            var dataModel = (TerrariaDataModel) DataModel;
            dataModel.Hp = _memory.ReadInt32(playerPointer + 0x340);
            dataModel.MaxHp = _memory.ReadInt32(playerPointer + 0x338);
            dataModel.Mana = _memory.ReadInt32(playerPointer + 0x344);
            dataModel.MaxMana = _memory.ReadInt32(playerPointer + 0x348);
            dataModel.Breath = _memory.ReadInt32(playerPointer + 0x2B4);
            dataModel.MaxBreath = _memory.ReadInt32(playerPointer + 0x2B0);
            dataModel.InWater = Convert.ToBoolean(_memory.ReadInt32(playerPointer + 0x1D));
            dataModel.InLava = Convert.ToBoolean(_memory.ReadInt32(playerPointer + 0x20));
        }
    }
}

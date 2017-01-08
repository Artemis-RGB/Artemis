using System;
using System.Linq;
using Artemis.DAL;
using Artemis.Managers;
using Artemis.Modules.Abstract;
using Artemis.Modules.Games.WoW.Data;
using Artemis.Settings;
using Artemis.Utilities.Memory;
using Process.NET;
using Process.NET.Memory;

namespace Artemis.Modules.Games.WoW
{
    public class WoWModel : ModuleModel
    {
        private readonly GamePointersCollection _pointer;
        private ProcessSharp _process;


        public WoWModel(DeviceManager deviceManager, LuaManager luaManager) : base(deviceManager, luaManager)
        {
            Settings = SettingsProvider.Load<WoWSettings>();
            DataModel = new WoWDataModel();
            ProcessName = "Wow-64";

            // Currently WoW is locked behind a hidden trigger (obviously not that hidden since you're reading this)
            // It is using memory reading and lets first try to contact Blizzard
            var settings = SettingsProvider.Load<GeneralSettings>();
            Settings.IsEnabled = settings.GamestatePort == 62575 && Settings.IsEnabled;

            _pointer = SettingsProvider.Load<OffsetSettings>().WorldOfWarcraft;
            //_pointer = new GamePointersCollection
            //{
            //    Game = "WorldOfWarcraft",
            //    GameVersion = "7.0.3.22810",
            //    GameAddresses = new List<GamePointer>
            //    {
            //        new GamePointer
            //        {
            //            Description = "ObjectManager",
            //            BasePointer = new IntPtr(0x1578070)
            //        },
            //        new GamePointer
            //        {
            //            Description = "LocalPlayer",
            //            BasePointer = new IntPtr(0x169DF10)
            //        },
            //        new GamePointer
            //        {
            //            Description = "NameCache",
            //            BasePointer = new IntPtr(0x151DCE8)
            //        },
            //        new GamePointer
            //        {
            //            Description = "TargetGuid",
            //            BasePointer = new IntPtr(0x179C940)
            //        }
            //    }
            //};
            //var res = JsonConvert.SerializeObject(_pointer, Formatting.Indented);
        }

        public override string Name => "WoW";
        public override bool IsOverlay => false;
        public override bool IsBoundToProcess => true;

        public override void Dispose()
        {
            base.Dispose();

            _process?.Dispose();
            _process = null;
        }

        public override void Update()
        {
            if (_process == null)
            {
                var tempProcess = MemoryHelpers.GetProcessIfRunning(ProcessName);
                if (tempProcess == null)
                    return;

                _process = new ProcessSharp(tempProcess, MemoryType.Remote);
            }

            if (ProfileModel == null || DataModel == null || _process == null)
                return;

            var dataModel = (WoWDataModel) DataModel;

            var objectManager = new WoWObjectManager(_process,
                _pointer.GameAddresses.First(a => a.Description == "ObjectManager").BasePointer);
            var nameCache = new WoWNameCache(_process,
                _pointer.GameAddresses.First(a => a.Description == "NameCache").BasePointer);
            var player = new WoWPlayer(_process,
                _pointer.GameAddresses.First(a => a.Description == "LocalPlayer").BasePointer,
                _pointer.GameAddresses.First(a => a.Description == "TargetGuid").BasePointer, true);

            dataModel.Player = player;
            if (dataModel.Player != null && dataModel.Player.Guid != Guid.Empty)
            {
                dataModel.Player.UpdateDetails(nameCache);
                var target = player.GetTarget(objectManager);
                if (target == null)
                    return;

                dataModel.Target = new WoWUnit(target.Process, target.BaseAddress);
                dataModel.Target.UpdateDetails(nameCache);
            }
            else
            {
                dataModel.Target = null;
            }
        }
    }
}
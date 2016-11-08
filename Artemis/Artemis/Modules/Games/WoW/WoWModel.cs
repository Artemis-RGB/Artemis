using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.DAL;
using Artemis.Managers;
using Artemis.Models;
using Artemis.Modules.Games.WoW.Data;
using Artemis.Profiles.Layers.Models;
using Artemis.Settings;
using Artemis.Utilities.Memory;
using Process.NET;
using Process.NET.Memory;

namespace Artemis.Modules.Games.WoW
{
    public class WoWModel : GameModel
    {
        private readonly GamePointersCollection _pointer;
        private ProcessSharp _process;

        public WoWModel(DeviceManager deviceManager)
            : base(deviceManager, SettingsProvider.Load<WoWSettings>(), new WoWDataModel())
        {
            Name = "WoW";
            ProcessName = "Wow-64";
            Scale = 4;

            // Currently WoW is locked behind a hidden trigger (obviously not that hidden if you're reading this)
            // It is using memory reading and lets first try to contact Blizzard
            var settings = SettingsProvider.Load<GeneralSettings>();
            Enabled = (settings.GamestatePort == 62575) && Settings.Enabled;

            Initialized = false;

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

        public int Scale { get; set; }

        public override void Dispose()
        {
            Initialized = false;

            _process.Dispose();
            _process = null;
            base.Dispose();
        }

        public override void Enable()
        {
            var tempProcess = MemoryHelpers.GetProcessIfRunning(ProcessName);
            if (tempProcess == null)
                return;

            _process = new ProcessSharp(tempProcess, MemoryType.Remote);

            Initialized = true;
        }

        public override void Update()
        {
            if ((Profile == null) || (DataModel == null) || (_process == null))
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
            if ((dataModel.Player != null) && (dataModel.Player.Guid != Guid.Empty))
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

        public override List<LayerModel> GetRenderLayers(bool keyboardOnly)
        {
            return Profile.GetRenderLayers(DataModel, keyboardOnly);
        }
    }
}
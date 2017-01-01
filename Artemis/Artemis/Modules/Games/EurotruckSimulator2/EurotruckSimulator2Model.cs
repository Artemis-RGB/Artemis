using System;
using System.Collections.Generic;
using System.IO;
using Artemis.DAL;
using Artemis.Managers;
using Artemis.Models;
using Artemis.Modules.Games.EurotruckSimulator2.Data;
using Artemis.Profiles.Layers.Models;
using Artemis.Properties;
using Artemis.Services;
using Artemis.Settings;
using Artemis.Utilities;
using Ninject.Extensions.Logging;

namespace Artemis.Modules.Games.EurotruckSimulator2
{
    public class EurotruckSimulator2Model : GameModel
    {
        private readonly MetroDialogService _dialogService;

        public EurotruckSimulator2Model(DeviceManager deviceManager, LuaManager luaManager,
            MetroDialogService dialogService)
            : base(deviceManager, luaManager, SettingsProvider.Load<EurotruckSimulator2Settings>(),
                new EurotruckSimulator2DataModel())
        {
            _dialogService = dialogService;
            Name = "EurotruckSimulator2";
            ProcessName = "eurotrucks2";
            Scale = 4;
            Enabled = Settings.Enabled;
            Initialized = false;

            FindGameDir();
        }

        public int Scale { get; set; }

        public override void Dispose()
        {
            Initialized = false;
            base.Dispose();
        }

        public override void Enable()
        {
            base.Enable();

            Initialized = true;
        }

        public override void Update()
        {
            var dataModel = (EurotruckSimulator2DataModel) DataModel;
            var telemetryData = Ets2TelemetryDataReader.Instance.Read();

            dataModel.Game = telemetryData.Game;
            dataModel.Job = telemetryData.Job;
            dataModel.Navigation = telemetryData.Navigation;
            dataModel.Trailer = telemetryData.Trailer;
            dataModel.Truck = telemetryData.Truck;
        }

        public void FindGameDir()
        {
            var gameSettings = (EurotruckSimulator2Settings) Settings;
            // Demo is also supported but resides in a different directory, the full game can also be 64-bits
            var dir = GeneralHelpers.FindSteamGame(@"\Euro Truck Simulator 2\bin\win_x64\eurotrucks2.exe") ??
                      GeneralHelpers.FindSteamGame(@"\Euro Truck Simulator 2\bin\win_x86\eurotrucks2.exe") ??
                      GeneralHelpers.FindSteamGame(@"\Euro Truck Simulator 2 Demo\bin\win_x86\eurotrucks2.exe");

            if (string.IsNullOrEmpty(dir))
                return;

            gameSettings.GameDirectory = dir;
            gameSettings.Save();

            if (!File.Exists(dir + "/plugins/ets2-telemetry-server.dll"))
                PlacePlugin();
        }

        public void PlacePlugin()
        {
            var gameSettings = (EurotruckSimulator2Settings) Settings;
            if (gameSettings.GameDirectory == string.Empty)
                return;

            var path = gameSettings.GameDirectory;

            // Ensure the selected directory exists
            if (!Directory.Exists(path))
            {
                _dialogService.ShowErrorMessageBox($"Directory '{path}' not found.");
                return;
            }
            // Ensure it's the ETS2 directory by looking for the executable
            if (!File.Exists(path + "/eurotrucks2.exe"))
            {
                _dialogService.ShowErrorMessageBox("Please select a valid Eurotruck Simulator 2 directory\n\n" +
                                                   @"By default ETS2 is in \SteamApps\common\Euro Truck Simulator 2\bin\win_x64");
                return;
            }

            // Create the plugins folder if it's not already there
            Directory.CreateDirectory(path + "/plugins");

            // Place either the 64-bits or 32-bits DLL
            try
            {
                if (path.Contains("win_x64"))
                    File.WriteAllBytes(path + "/plugins/ets2-telemetry-server.dll", Resources.ets2_telemetry_server_x64);
                else
                    File.WriteAllBytes(path + "/plugins/ets2-telemetry-server.dll", Resources.ets2_telemetry_server_x86);

                Logger?.Debug("Installed ETS2 plugin in {0}", path);
            }
            catch (Exception e)
            {
                Logger?.Error(e, "Failed to install ETS2 plugin in {0}", path);
                throw;
            }
        }

        public override List<LayerModel> GetRenderLayers(bool keyboardOnly)
        {
            return Profile.GetRenderLayers(DataModel, keyboardOnly);
        }
    }
}
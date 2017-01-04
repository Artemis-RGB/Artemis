using System;
using System.IO;
using Artemis.DAL;
using Artemis.Managers;
using Artemis.Modules.Abstract;
using Artemis.Modules.Games.EurotruckSimulator2.Data;
using Artemis.Properties;
using Artemis.Services;
using Artemis.Utilities;

namespace Artemis.Modules.Games.EurotruckSimulator2
{
    public class EurotruckSimulator2Model : ModuleModel
    {
        private readonly MetroDialogService _dialogService;


        public EurotruckSimulator2Model(DeviceManager deviceManager, LuaManager luaManager,
            MetroDialogService dialogService) : base(deviceManager, luaManager)
        {
            _dialogService = dialogService;

            Settings = SettingsProvider.Load<EurotruckSimulator2Settings>();
            DataModel = new EurotruckSimulator2DataModel();
            ProcessName = "eurotrucks2";

            FindGameDir();
        }

        public override string Name => "EurotruckSimulator2";
        public override bool IsOverlay => false;
        public override bool IsBoundToProcess => true;

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
            // Demo is also supported but resides in a different directory, the full game can also be 64-bits
            var dir = GeneralHelpers.FindSteamGame(@"\Euro Truck Simulator 2\bin\win_x64\eurotrucks2.exe") ??
                      GeneralHelpers.FindSteamGame(@"\Euro Truck Simulator 2\bin\win_x86\eurotrucks2.exe") ??
                      GeneralHelpers.FindSteamGame(@"\Euro Truck Simulator 2 Demo\bin\win_x86\eurotrucks2.exe");

            if (string.IsNullOrEmpty(dir))
                return;

            ((EurotruckSimulator2Settings) Settings).GameDirectory = dir;
            Settings.Save();

            if (!File.Exists(dir + "/plugins/ets2-telemetry-server.dll"))
                PlacePlugin();
        }

        public void PlacePlugin()
        {
            if (((EurotruckSimulator2Settings) Settings).GameDirectory == string.Empty)
                return;

            var path = ((EurotruckSimulator2Settings) Settings).GameDirectory;

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
    }
}
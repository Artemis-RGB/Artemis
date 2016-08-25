using System.Collections.Generic;
using Artemis.DAL;
using Artemis.Managers;
using Artemis.Models;
using Artemis.Profiles.Layers.Models;
using Ets2SdkClient;
using Ninject.Extensions.Logging;

namespace Artemis.Modules.Games.EurotruckSimulator2
{
    public class EurotruckSimulator2Model : GameModel
    {
        public EurotruckSimulator2Model(MainManager mainManager)
            : base(mainManager, SettingsProvider.Load<EurotruckSimulator2Settings>(), new EurotruckSimulator2DataModel()
            )
        {
            Name = "EurotruckSimulator2";
            ProcessName = "eurotrucks2";
            Scale = 4;
            Enabled = Settings.Enabled;
            Initialized = false;
        }

        public ILogger Logger { get; set; }
        public int Scale { get; set; }

        public Ets2SdkTelemetry Telemetry { get; set; }

        public override void Dispose()
        {
            Initialized = false;

            Telemetry.Data -= TelemetryOnData;
            Telemetry = null;
        }

        public override void Enable()
        {
            Telemetry = new Ets2SdkTelemetry();
            Telemetry.Data += TelemetryOnData;
            if (Telemetry.Error != null)
                MainManager.Logger.Error(Telemetry.Error, "Exception in the Eurotruck SDK");

            Initialized = true;
        }

        private void TelemetryOnData(Ets2Telemetry data, bool newTimestamp)
        {
            ((EurotruckSimulator2DataModel) DataModel).Axilliary = data.Axilliary;
            ((EurotruckSimulator2DataModel) DataModel).Controls = data.Controls;
            ((EurotruckSimulator2DataModel) DataModel).Damage = data.Damage;
            ((EurotruckSimulator2DataModel) DataModel).Drivetrain = data.Drivetrain;
            ((EurotruckSimulator2DataModel) DataModel).Job = data.Job;
            ((EurotruckSimulator2DataModel) DataModel).Lights = data.Lights;
            ((EurotruckSimulator2DataModel) DataModel).Manufacturer = data.Manufacturer;
            ((EurotruckSimulator2DataModel) DataModel).ManufacturerId = data.ManufacturerId;
            ((EurotruckSimulator2DataModel) DataModel).Physics = data.Physics;
        }

        public override void Update()
        {
            // Updating is handled in the TelemetryOnData event
        }

        public override List<LayerModel> GetRenderLayers(bool keyboardOnly)
        {
            return Profile.GetRenderLayers(DataModel, keyboardOnly);
        }
    }
}
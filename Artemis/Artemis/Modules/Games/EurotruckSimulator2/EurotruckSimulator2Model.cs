using System.Collections.Generic;
using Artemis.DAL;
using Artemis.Managers;
using Artemis.Models;
using Artemis.Modules.Games.EurotruckSimulator2.Data;
using Artemis.Profiles.Layers.Models;
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

        public override void Dispose()
        {
            Initialized = false;
            base.Dispose();
        }

        public override void Enable()
        {
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

        public override List<LayerModel> GetRenderLayers(bool keyboardOnly)
        {
            return Profile.GetRenderLayers(DataModel, keyboardOnly);
        }
    }
}
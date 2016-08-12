using System.Collections.Generic;
using Artemis.Managers;
using Artemis.Models;
using Artemis.Profiles.Layers.Models;
using Artemis.Utilities.Memory;

namespace Artemis.Modules.Games.WorldofWarcraft
{
    public class WoWModel : GameModel
    {
        private Memory _memory;

        public WoWModel(MainManager mainManager, WoWSettings settings)
            : base(mainManager, settings, new WoWDataModel())
        {
            Name = "WoW";
            ProcessName = "Wow-64";
            Scale = 4;
            Enabled = Settings.Enabled;
            Initialized = false;
        }

        public int Scale { get; set; }

        public override void Dispose()
        {
            Initialized = false;
        }

        public override void Enable()
        {
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

//            _memory.ReadMemory();
        }

        public override List<LayerModel> GetRenderLayers(bool keyboardOnly)
        {
            return Profile.GetRenderLayers(DataModel, keyboardOnly);
        }
    }
}
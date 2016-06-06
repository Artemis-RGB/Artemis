using System.Collections.Generic;
using Artemis.Managers;
using Artemis.Models;
using Artemis.Models.Profiles;

namespace Artemis.Modules.Effects.WindowsProfile
{
    public class WindowsProfileModel : EffectModel
    {
        public WindowsProfileModel(MainManager mainManager, WindowsProfileSettings settings)
            : base(mainManager, new WindowsProfileDataModel())
        {
            Name = "WindowsProfile";
            Settings = settings;
        }

        public WindowsProfileSettings Settings { get; set; }

        public override void Dispose()
        {
            Initialized = false;
        }

        public override void Enable()
        {
            Initialized = true;
        }

        public override void Update()
        {
        }

        public override List<LayerModel> GetRenderLayers(bool renderMice, bool renderHeadsets)
        {
            return Profile.GetRenderLayers<WindowsProfileDataModel>(DataModel, renderMice, renderHeadsets, true);
        }
    }
}
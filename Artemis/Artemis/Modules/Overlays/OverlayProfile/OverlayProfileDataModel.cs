using Artemis.Modules.Abstract;
using Artemis.Modules.General.GeneralProfile;
using MoonSharp.Interpreter;

namespace Artemis.Modules.Overlays.OverlayProfile
{
    public class OverlayProfileDataModel : ModuleDataModel
    {
        public OverlayProfileDataModel()
        {
            Keyboard = new KbDataModel();
            Audio = new Audio();
        }

        public KbDataModel Keyboard { get; set; }
        public Audio Audio { get; set; }
    }

    [MoonSharpUserData]
    public class Audio
    {
        public float Volume { get; set; }
    }
}
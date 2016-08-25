using Artemis.Managers;
using Artemis.Settings;

namespace Artemis.Models
{
    public abstract class OverlayModel : EffectModel
    {
        private bool _enabled;
        public string ProcessName;

        protected OverlayModel(MainManager mainManager, OverlaySettings settings) : base(mainManager, settings, null)
        {
            Settings = settings;
            Enabled = settings.Enabled;
        }

        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                if (_enabled == value)
                    return;

                if (value)
                    Enable();
                else
                    Dispose();
                _enabled = value;
            }
        }

        public new OverlaySettings Settings { get; set; }
        public abstract void RenderOverlay(RenderFrame frame, bool keyboardOnly);
    }
}
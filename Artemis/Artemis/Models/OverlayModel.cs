using System.Drawing;

namespace Artemis.Models
{
    public abstract class OverlayModel : EffectModel
    {
        private bool _enabled;
        public string ProcessName;

        protected OverlayModel(MainModel mainModel) : base(mainModel)
        {
        }

        // Overlay Enabled() and Dispose() is called when changing the Enabled value
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

        public void SetEnabled(bool enabled)
        {
            if (Enabled == enabled)
                return;

            if (enabled)
                Enable();
            else
                Dispose();

            Enabled = enabled;
        }

        public abstract Bitmap GenerateBitmap(Bitmap bitmap);
    }
}
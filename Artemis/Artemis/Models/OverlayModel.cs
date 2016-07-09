﻿using Artemis.Managers;

namespace Artemis.Models
{
    public abstract class OverlayModel : EffectModel
    {
        private bool _enabled;
        public string ProcessName;

        protected OverlayModel(MainManager mainManager) : base(mainManager, null)
        {
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

        public abstract void RenderOverlay(RenderFrame frame, bool keyboardOnly);
    }
}
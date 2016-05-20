using System;
using System.Drawing;
using Artemis.Managers;
using Brush = System.Windows.Media.Brush;

namespace Artemis.Models
{
    public abstract class EffectModel : IDisposable
    {
        public delegate void SettingsUpdateHandler(EffectSettings settings);

        public bool Initialized;
        public MainManager MainManager;
        public string Name;

        protected EffectModel(MainManager mainManager)
        {
            MainManager = mainManager;
        }

        public abstract void Dispose();

        // Called on creation
        public abstract void Enable();

        // Called every iteration
        public abstract void Update();

        // Called after every update
        public abstract Bitmap GenerateBitmap();

        public abstract Brush GenerateMouseBrush();
        public abstract Brush GenerateHeadsetBrush();
    }
}
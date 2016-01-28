using System;
using System.Drawing;

namespace Artemis.Models
{
    public abstract class EffectModel : IDisposable
    {
        public MainModel MainModel;
        public string Name;

        protected EffectModel(MainModel mainModel)
        {
            MainModel = mainModel;
        }

        public abstract void Dispose();

        // Called on creation
        public abstract void Enable();

        // Called every iteration
        public abstract void Update();

        // Called after every update
        public abstract Bitmap GenerateBitmap();
    }
}
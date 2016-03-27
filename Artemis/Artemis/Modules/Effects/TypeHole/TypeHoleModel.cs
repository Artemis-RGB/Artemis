using System.Drawing;
using Artemis.Managers;
using Artemis.Models;

namespace Artemis.Modules.Effects.TypeHole
{
    public class TypeHoleModel : EffectModel
    {
        public TypeHoleModel(MainManager mainManager) : base(mainManager)
        {
            Name = "TypeHole";
            Initialized = false;
        }

        public override void Dispose()
        {
            Initialized = false;

            // Disable logic
        }

        public override void Enable()
        {
            Initialized = false;

            // Enable logic

            Initialized = true;
        }

        public override void Update()
        {
        }

        public override Bitmap GenerateBitmap()
        {
            return null;
        }
    }
}
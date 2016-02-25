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
        }

        public override void Dispose()
        {
        }

        public override void Enable()
        {
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
using Artemis.Managers;
using Artemis.Modules.Abstract;
using Ninject;

namespace Artemis.Modules.Games.FormulaOne2017
{
    public sealed class FormulaOne2017ViewModel : ModuleViewModel
    {
        public FormulaOne2017ViewModel(MainManager mainManager, [Named(nameof(FormulaOne2017Model))] ModuleModel moduleModel,
            IKernel kernel) : base(mainManager, moduleModel, kernel)
        {
            DisplayName = "F1 2017";
        }

        public override bool UsesProfileEditor => true;
    }
}